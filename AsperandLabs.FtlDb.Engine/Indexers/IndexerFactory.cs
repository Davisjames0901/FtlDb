using AsperandLabs.FtlDb.Engine.Indexers.Composite;
using AsperandLabs.FtlDb.Engine.Row;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AsperandLabs.FtlDb.Engine.Indexers;

public class IndexerFactory
{
    public IPrimaryIndexer GetPrimaryIndexer(ColumnSpec spec, string tableDir)
    {
        //Maybe later here we can decide if a column is auto increment or not?
        return spec.ColumnValueType switch
        {
            "System.Int32" => new Int32Unique(tableDir, spec.ColumnName),
            _ => throw new NotImplementedException($"Index type {spec.ColumnType}:{spec.ColumnValueType} is not supported")
        };
    }
    
    public IIndexer GetCompositeIndexer(ColumnSpec spec, string tableDir)
    {
        //Later we can use the column spec to get unique vs non-unique and other variants.
        return spec.ColumnValueType switch
        {
            "System.Double" => new NotUnique<double>(tableDir, spec.ColumnName),
            _ => throw new NotImplementedException($"Index type {spec.ColumnType}:{spec.ColumnValueType} is not supported")
        };
    }
}