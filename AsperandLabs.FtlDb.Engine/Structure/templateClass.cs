namespace AsperandLabs.FtlDb.Engine.Structure;

public class templateClass : IRow
{
    public string Test { get; set; }
    
    public bool GetProp(string name, out object? o)
    {
        o = null;
        switch (name)
        {
            case "Test":
                o = Test;
                return true;
            default:
                return false;
        }
    }

    public bool SetProp(string name, object o)
    {
        switch (name)
        {
            case "Test":
                Test = (string)o;
                return true;
            default:
                return false;
        }
    }
}