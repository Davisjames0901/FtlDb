using AsperandLabs.FtlDb.Engine.Row;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AsperandLabs.FtlDb.Engine.Indexers;

public class IndexerFactory
{
    public IIndexer GetIndexer(ColumnSpec spec, string tableDir)
    {
        return (spec.ColumnType, spec.ColumnValueType) switch
        {
            (IndexType.PrimaryKey, "System.Int32") => new Int32Unique(tableDir, spec.ColumnName),
            _ => throw new NotImplementedException($"Index type {spec.ColumnType}:{spec.ColumnValueType} is not supported")
        };
    }
}