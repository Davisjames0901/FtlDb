using AsperandLabs.FtlDb.Engine.Row;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AsperandLabs.FtlDb.Engine.Indexers;

public class IndexerFactory
{
    public IPrimaryIndexer GetPrimaryIndexer(ColumnSpec spec, string tableDir)
    {
        return spec.ColumnValueType switch
        {
            "System.Int32" => new Int32Unique(tableDir, spec.ColumnName),
            _ => throw new NotImplementedException($"Index type {spec.ColumnType}:{spec.ColumnValueType} is not supported")
        };
    }
    
    public IIndexer GetIndexer(ColumnSpec spec, string tableDir)
    {
        //We need some indexers...
        return spec.ColumnValueType switch
        {
            _ => throw new NotImplementedException($"Index type {spec.ColumnType}:{spec.ColumnValueType} is not supported")
        };
    }
}