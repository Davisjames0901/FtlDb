namespace AsperandLabs.FtlDb.Engine.CodeGeneration;

public static class CSharpSnips
{
    
    public static string RowClassHeader(string schema, string name) => $@"using MessagePack;

namespace AsperandLabs.FtlDb.TestConsole.Schemas.{schema};

[MessagePackObject]
public class {name}
{{
";
    
    
    public static string RowClassProperty(int index, string name, Type type) => $"\t[Key({index})] public {type} {name} {{ get; set; }}";
    
    public const string ClassFooter = "\n}";
}