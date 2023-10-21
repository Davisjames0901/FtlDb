namespace AsperandLabs.FtlDb.Engine.Indexers;

public interface IIndexer: IDisposable
{
    bool TryMatch(object key, out (long start, int length)[] result);
    bool WriteKey(object key, long start, int length);
    int Count();
    Type GetKeyType();
}