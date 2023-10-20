namespace AsperandLabs.FtlDb.Engine.Indexers;

public interface IIndexer: IDisposable
{
    (long from, long to)? GetIndex(object key);
    object NextKey();
    bool WriteKey(object key, long from, long to);
    int Count();
    Type GetKeyType();
}