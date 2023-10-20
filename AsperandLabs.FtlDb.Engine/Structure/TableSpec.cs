namespace AsperandLabs.FtlDb.Engine.Row;

public class TableSpec
{
    public string Name { get; set; } = string.Empty;
    public string Schema { get; set; } = "dbo";
    public List<ColumnSpec> Columns { get; set; } = new List<ColumnSpec>();
}