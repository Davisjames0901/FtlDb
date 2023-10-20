// See https://aka.ms/new-console-template for more information

using System.Reflection;
using AsperandLabs.FtlDb.Engine;
using AsperandLabs.FtlDb.Engine.CodeGeneration;
using AsperandLabs.FtlDb.Engine.Row;
using MessagePack;

var gen = new RowClassGenerator();
Console.WriteLine("Constructing source...\n");
var spec = new TableSpec()
{
    Name = "TestTable",
    Columns = new List<ColumnSpec>()
    {
        new ColumnSpec {
            ColumnName = "Date",
            ColumnType = IndexType.PrimaryKey,
            ColumnValueType = "System.DateTime"
        },
        new ColumnSpec {
            ColumnName = "Value",
            ColumnType = IndexType.None,
            ColumnValueType = "System.Double" 
        }
    }
};
var classFile = gen.GetRowClassSource(spec);
Console.WriteLine("Compiling...\n");
var asmb = Assembly.Load(gen.CreateAssembly(classFile, "test"));

var type = asmb.GetType("AsperandLabs.FtlDb.TestConsole.Schemas.dbo.TestTable");
dynamic instance = asmb.CreateInstance("AsperandLabs.FtlDb.TestConsole.Schemas.dbo.TestTable");
Console.WriteLine(instance.ToString());
instance.Date = DateTime.Now;
instance.Value = 12.3d;
var bytes = MessagePackSerializer.Serialize(instance);

//into dynamic so we can access the properties
dynamic deserialized = MessagePackSerializer.Deserialize(type, bytes);
Console.WriteLine($"Deserialized Values - Date: {deserialized.Date}; Value: {deserialized.Value};");