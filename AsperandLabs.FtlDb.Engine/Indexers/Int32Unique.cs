using Microsoft.Win32.SafeHandles;

namespace AsperandLabs.FtlDb.Engine.Indexers;

public class Int32Unique : IIndexer
{
    private readonly string _keyFilePath;
    private readonly FileStream _keyFile;
    private int _next;

    private const int ROW_WIDTH = 16;
    public Int32Unique(string tableDir, string name)
    {
        _keyFilePath = Path.Combine(tableDir, nameof(Int32Unique) + name + ".inx");
        if (!File.Exists(_keyFilePath))
            _keyFile = File.Create(_keyFilePath);
        else
        {
            _keyFile = File.Open(_keyFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }
    }

    public (long from, long to)? GetIndex(object key)
    {
        var id = (int)key;
        var buffer = new byte[ROW_WIDTH];
        var position = id * ROW_WIDTH;
        if (position > _keyFile.Length)
            return null;

        _keyFile.Seek(position, SeekOrigin.Begin);
        var byteCount = _keyFile.Read(buffer, 0, ROW_WIDTH);
        //If dont read 8 bytes, the id doesnt exist
        if (byteCount != ROW_WIDTH)
            return null;

        var span = new Span<byte>(buffer);
        var from = BitConverter.ToInt64(span.Slice(0, 8));
        var to = BitConverter.ToInt32(span.Slice(8, 8));
        return (from, to);
    }ga

    public object NextKey()
    {
        return (int)(_keyFile.Length/ROW_WIDTH);
    }
    
    public int Count()
    {
        return (int)(_keyFile.Length/ROW_WIDTH);
    }

    public Type GetKeyType()
    {
        return typeof(int);
    }

    public bool WriteKey(object key, long from, long to)
    {
        var id = (int)key;
        var buffer = new byte[ROW_WIDTH];
        var position = id * ROW_WIDTH;

        if (position < _keyFile.Length)
        {
            var byteCount = _keyFile.Read(buffer, position, ROW_WIDTH);
            if (buffer.Any(x => x != 0))
                return false;
        }
        if (position >= _keyFile.Length)
        {
            Array.Copy(BitConverter.GetBytes(from), 0, buffer, 0, 8);
            Array.Copy(BitConverter.GetBytes(to), 0, buffer, 8, 8);
            _keyFile.Seek(0, SeekOrigin.End);
            _keyFile.Write(buffer);
            _keyFile.Flush();
            return true;
        }
        
        Array.Copy(BitConverter.GetBytes(from), 0, buffer, 0, 8);
        Array.Copy(BitConverter.GetBytes(to), 0, buffer, 8, 8);
        
        _keyFile.Write(buffer, position, ROW_WIDTH);
        _keyFile.Flush();
        return true;
    }

    public void Dispose()
    {
        _keyFile?.Dispose();
    }
}