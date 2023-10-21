namespace AsperandLabs.FtlDb.Engine.Indexers;

public interface IIndexer: IDisposable
{
    Type GetKeyType();
    bool TryMatch(object key, out int[] results);
    bool WriteKey(object key, int primarySequence);
}