namespace AsperandLabs.FtlDb.Engine.Config;

public class TableConfig
{
    public string BaseDir { get; set; }
    public string BaseSchemaDir => Path.Combine(BaseDir, "Schemas");
    public string SchemaDir(string schema) => Path.Combine(BaseSchemaDir, schema);
    public string TableDir(string name, string schema) => Path.Combine(BaseSchemaDir, schema, name);
    public string TableSpecFile(string name, string schema) => Path.Combine(TableDir(name, schema), "spec.json");
    public string TableSourceFile(string name, string schema) => Path.Combine(TableDir(name, schema), "source.cs");
    public string TableAssmFile(string name, string schema) => Path.Combine(TableDir(name, schema), "assembly.dll");
    public string TableDataFile(string name, string schema) => Path.Combine(TableDir(name, schema), "data.dat");
    public string GetTableClassName(string name, string schema) => $"AsperandLabs.FtlDb.TestConsole.Schemas.{schema}.{name}";
}