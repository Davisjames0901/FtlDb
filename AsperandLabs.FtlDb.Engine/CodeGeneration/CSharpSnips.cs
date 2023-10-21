using System.Text;
using AsperandLabs.FtlDb.Engine.Row;

namespace AsperandLabs.FtlDb.Engine.CodeGeneration;

public static class CSharpSnips
{
    
    public static string RowClassHeader(string schema, string name) => $@"using MessagePack;
using AsperandLabs.FtlDb.Engine.Structure;

namespace AsperandLabs.FtlDb.TestConsole.Schemas.{schema};

[MessagePackObject]
public class {name} : IRow
{{
";
    
    
    public static string RowClassProperty(int index, string name, Type type) => $"\t[Key({index})] public {type} {name} {{ get; set; }}";

    public static string GetPropertyAccessor(List<ColumnSpec> columns)
    {
        var sb = new StringBuilder();

        sb.AppendLine("\tpublic bool GetProp(string name, out object? o)\n\t{");
        sb.AppendLine("\t\t o = null;");
        sb.AppendLine("\t\t switch (name)\n\t\t{");
        foreach (var column in columns)
        {
            sb.AppendLine($"\t\t\t case \"{column.ColumnName}\": \n\t\t\t\t o = {column.ColumnName}; \n\t\t\t\t return true;");
        }
        
        sb.AppendLine("\t\t\t default: \n\t\t\t\t return false;");
        sb.AppendLine("\t\t}");
        sb.AppendLine("\t}");
        
        return sb.ToString();
    }
    public static string GetPropertySetter(List<ColumnSpec> columns)
    {
        var sb = new StringBuilder();

        sb.AppendLine("\tpublic bool SetProp(string name, object? o)\n\t{");
        sb.AppendLine("\t\t switch (name)\n\t\t{");
        foreach (var column in columns)
        {
            sb.AppendLine($"\t\t\t case \"{column.ColumnName}\": \n\t\t\t\t {column.ColumnName} = ({column.ColumnValueType})o; \n\t\t\t\t return true;");
        }
        
        sb.AppendLine("\t\t\t default: \n\t\t\t\t return false;");
        sb.AppendLine("\t\t}");
        sb.AppendLine("\t}");
        
        return sb.ToString();
    }
    
    public const string ClassFooter = "}";
}