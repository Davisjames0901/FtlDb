namespace AsperandLabs.FtlDb.Engine.Indexers;

public interface IPrimaryIndexer
{
    object NextKey();
    int GetKeySequence(object key);
    bool TryMatch(object key, out (long start, int length) result);
    bool TryMatchSequence(int key, out (long start, int length) result);
    bool WriteKey(object key, long start, int length);
    Type GetKeyType();
    int Count();
}