using AsperandLabs.FtlDb.Engine.Helpers;

namespace AsperandLabs.FtlDb.Engine.Indexers.Composite;

public class NotUnique<T> : IIndexer where T : unmanaged
{
    private readonly FileStream _keyFile;
    private int RowWidth = TypeHelpers.GetSizeOf<T>() + TypeHelpers.GetSizeOf<int>();
    private readonly Dictionary<T, List<int>> _inxLookup;
    
    public NotUnique(string tableDir, string name)
    {
        var keyFilePath = Path.Combine(tableDir, nameof(NotUnique<T>) + name + ".inx");
        _keyFile = File.Open(keyFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        _inxLookup = new Dictionary<T, List<int>>();
    }
    
    public Type GetKeyType()
    {
        return typeof(T);
    }

    public bool TryMatch(object keyObj, out int[] results)
    {
        var key = (T)keyObj;
        if (!_inxLookup.ContainsKey(key))
        {
            results = Array.Empty<int>();
            return false;
        }

        results = _inxLookup[key].ToArray();
        return true;
    }

    public bool WriteKey(object keyObj, int primarySequence)
    {
        var key = (T)keyObj;
        TryAddRecord(key, primarySequence);
        return true;
    }

    private void TryAddRecord(T key, int sequence)
    {
        if(!_inxLookup.ContainsKey(key))
            _inxLookup.Add(key, new List<int>());
        _inxLookup[key].Add(sequence);
    }
    
    public void Dispose()
    {
        
    }

}