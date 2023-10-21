namespace AsperandLabs.FtlDb.Engine.Structure;

public interface IRow
{
    bool GetProp(string name, out object? o);
    bool SetProp(string name, object o);
}