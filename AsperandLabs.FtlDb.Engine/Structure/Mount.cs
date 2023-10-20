using System.Reflection;
using AsperandLabs.FtlDb.Engine.Config;
using AsperandLabs.FtlDb.Engine.Indexers;
using AsperandLabs.FtlDb.Engine.Services;
using MessagePack;

namespace AsperandLabs.FtlDb.Engine.Row;

public class Mount : IDisposable
{
    private readonly TableConfig _config;
    private readonly TableService _tableService;
    private IndexerFactory _indexFactory;
    private Type _rowClass;
    private FileStream _dataFile;
    private IIndexer _primaryKey;

    public Mount(TableConfig config, TableService tableService, IndexerFactory indexFactory)
    {
        _config = config;
        _tableService = tableService;
        _indexFactory = indexFactory;
    }

    public object? ReadByPrimaryKey(object key)
    {
        var index = _primaryKey.GetIndex(key);
        if (index == null)
            return null;

        var size = (int)(index.Value.to - index.Value.from);
        var buffer = new byte[size];
        _dataFile.Seek((int)index.Value.from, SeekOrigin.Begin);
        var read = _dataFile.Read(buffer, 0, size);
        if (read != size)
            return null;

        return MessagePackSerializer.Deserialize(_rowClass, buffer);
    }
    
    
    public object? WriteRow(object row)
    {
        var bytes = MessagePackSerializer.Serialize(_rowClass, row);
        var id = _primaryKey.NextKey();
        _dataFile.Seek(0, SeekOrigin.End);
        var from = _dataFile.Position;
        var to = from + bytes.Length;

        if (!_primaryKey.WriteKey(id, from, to))
            return null;
        _dataFile.Write(bytes);
        _dataFile.Flush();
        return id;
    }

    public int Count()
    {
        return _primaryKey.Count();
    }

    public Type RowType()
    {
        return _rowClass;
    }

    public Type GetIndexType(ColumnSpec columnSpec)
    {
        //Only primary key for now
        return _primaryKey.GetKeyType();
    }
    
    public bool Init(string table, string schema)
    {
        var tableSpec = _tableService.GetTableSpec(table, schema);
        if (tableSpec == null)
            return false;
        
        var primaryColumn = tableSpec.Columns.Single(x => x.ColumnType == IndexType.PrimaryKey);
        _primaryKey = _indexFactory.GetIndexer(primaryColumn, _config.TableDir(table, schema));

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
        _primaryKey.Dispose();
        _dataFile.Dispose();
    }
}