using System.Buffers;
using System.Reflection;
using AsperandLabs.FtlDb.Engine.Config;
using AsperandLabs.FtlDb.Engine.Indexers;
using AsperandLabs.FtlDb.Engine.Services;
using AsperandLabs.FtlDb.Engine.Structure;
using MessagePack;

namespace AsperandLabs.FtlDb.Engine.Row;

public class Mount : IDisposable
{
    private readonly TableConfig _config;
    private readonly TableService _tableService;
    private IndexerFactory _indexFactory;
    private Type _rowClass;
    private FileStream _dataFile;
    private Dictionary<string, IIndexer> _indices;
    private (ColumnSpec Column, IPrimaryIndexer Indexer) _primaryIndex;
    public TableSpec TableSpec { get; private set; }

    public Mount(TableConfig config, TableService tableService, IndexerFactory indexFactory)
    {
        _config = config;
        _tableService = tableService;
        _indexFactory = indexFactory;
        _indices = new Dictionary<string, IIndexer>();
    }

    public object[] ReadFirstByIndex(string indexName, object indexKey)
    {
        if (!_indices.TryGetValue(indexName, out var index))
            return Array.Empty<object>();

        if (!index.TryMatch(indexKey, out var results))
            return Array.Empty<object>();

        var rows = new object[results.Length];
        for (var i = 0; i < results.Length; i++)
        {
            
            var buffer = new byte[results[i].length];
            _dataFile.Seek((int)results[i].start, SeekOrigin.Begin);
            var read = _dataFile.ReadAsync(buffer, 0, results[i].length);

            rows[i] = MessagePackSerializer.Deserialize(_rowClass, buffer);
        }

        return rows;
    }
    
    
    public object? WriteRow(object row)
    {
        var rowAccessor = (IRow)row;
        //Get next available Id and set it on the class before serializing
        var id = _primaryIndex.Indexer.NextKey();
        rowAccessor.SetProp(_primaryIndex.Column.ColumnName, id);
        
        var bytes = MessagePackSerializer.Serialize(_rowClass, row);
        _dataFile.Seek(0, SeekOrigin.End);
        var start = _dataFile.Position;

        //TODO: we need to read all indexed values from the row accessor
        //write and write the index files.
        if (!_primaryIndex.Indexer.WriteKey(id, start, bytes.Length))
            return null;
        
        _dataFile.Write(bytes);
        _dataFile.WriteByte(0x0);
        _dataFile.Flush();
        return id;
    }

    public int Count()
    {
        return _primaryIndex.Indexer.Count();
    }

    public Type RowType()
    {
        return _rowClass;
    }

    public Type? GetIndexType(string columnName)
    {
        if (!_indices.TryGetValue(columnName, out var indexer))
            return null;
        return indexer.GetKeyType();
    }
    
    public bool Init(string table, string schema)
    {
        var tableSpec = _tableService.GetTableSpec(table, schema);
        if (tableSpec == null)
            return false;

        TableSpec = tableSpec;
        var tableDir = _config.TableDir(table, schema);
        foreach (var column in tableSpec.Columns)
        {
            //If there is no index, then there is no indexer to get
            if(column.ColumnType == IndexType.None)
                continue;
            
            if (column.ColumnType == IndexType.PrimaryKey)
            {
                var primaryIndex = _indexFactory.GetPrimaryIndexer(column, tableDir);
                _primaryIndex.Indexer = primaryIndex;
                _primaryIndex.Column = column;
                _indices.Add(column.ColumnName, primaryIndex);
            }
            else
            {
                var index = _indexFactory.GetIndexer(column, tableDir);
                _indices.Add(column.ColumnName, index);
            }
        }

        if (_primaryIndex.Indexer == null)
            return false;

        var dataPath = _config.TableDataFile(table, schema);
        if (!Path.Exists(dataPath))
            _dataFile = File.Create(dataPath);
        else
        {
            _dataFile = File.Open(dataPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }


        var assembly = File.ReadAllBytes(_config.TableAssmFile(table, schema));
        var loadedAssembly = Assembly.Load(assembly);
        var rowClass = loadedAssembly.GetType(_config.GetTableClassName(table, schema));

        if (rowClass == null)
            return false;

        _rowClass = rowClass;
        
        return true;
    }

    public void Dispose()
    {
        foreach (var index in _indices)
        {
            index.Value.Dispose();
        }
        _dataFile.Dispose();
    }
}