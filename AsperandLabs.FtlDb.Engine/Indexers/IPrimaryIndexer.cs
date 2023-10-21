namespace AsperandLabs.FtlDb.Engine.Indexers;

public interface IPrimaryIndexer : IIndexer
{
    object NextKey();
}