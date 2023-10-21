
namespace AsperandLabs.FtlDb.Engine.Indexers;

public class Int32Unique : IPrimaryIndexer
{
    private readonly FileStream _keyFile;
    private const int RowWidth = 12;

    public Int32Unique(string tableDir, string name)
    {
        var keyFilePath = Path.Combine(tableDir, nameof(Int32Unique) + name + ".inx");
        _keyFile = File.Open(keyFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
    }

    public bool TryMatch(object key, out (long start, int length) result)
    {
        var id = GetKeySequence(key);
        return TryMatchSequence(id, out result);
    }
    
    public bool TryMatchSequence(int id, out (long start, int length) result)
    {
        result = (0, 0);
        var buffer = new byte[RowWidth];
        var position = id * RowWidth;
        if (position > _keyFile.Length)
            return false;

        _keyFile.Seek(position, SeekOrigin.Begin);
        var byteCount = _keyFile.Read(buffer, 0, RowWidth);
        //If dont read 8 bytes, the id doesnt exist
        if (byteCount != RowWidth)
            return false;

        var span = new Span<byte>(buffer);
        var start = BitConverter.ToInt64(span.Slice(0, 8));
        var length = BitConverter.ToInt32(span.Slice(8, 4));
        result = (start, length);
        return true;
    }

    public object NextKey()
    {
        return _keyFile.Length / RowWidth;
    }

    public int GetKeySequence(object key)
    {
        return (int)key;
    }

    public int Count()
    {
        return (int)(_keyFile.Length / RowWidth);
    }

    public Type GetKeyType()
    {
        return typeof(int);
    }

    public bool WriteKey(object key, long start, int length)
    {
        var id = (int)key;
        var buffer = new byte[RowWidth];
        var position = id * RowWidth;

        if (position < _keyFile.Length)
        {
            _ = _keyFile.Read(buffer, position, RowWidth);
            if (buffer.Any(x => x != 0))
                return false;
        }

        if (position >= _keyFile.Length)
        {
            Array.Copy(BitConverter.GetBytes(start), 0, buffer, 0, 8);
            Array.Copy(BitConverter.GetBytes(length), 0, buffer, 8, 4);
            _keyFile.Seek(0, SeekOrigin.End);
            _keyFile.Write(buffer);
            _keyFile.Flush();
            return true;
        }

        Array.Copy(BitConverter.GetBytes(start), 0, buffer, 0, 8);
        Array.Copy(BitConverter.GetBytes(length), 0, buffer, 8, 4);

        _keyFile.Write(buffer, position, RowWidth);
        _keyFile.Flush();
        return true;
    }

    public void Dispose()
    {
        _keyFile.Dispose();
    }
}