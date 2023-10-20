using System.Text.Json;
using AsperandLabs.FtlDb.Common;
using AsperandLabs.FtlDb.Engine.CodeGeneration;
using AsperandLabs.FtlDb.Engine.Config;
using AsperandLabs.FtlDb.Engine.Row;
using AsperandLabs.FtlDb.Engine.Validators;
using Microsoft.Extensions.Logging;

namespace AsperandLabs.FtlDb.Engine.Services;

public class TableService
{
    private readonly ILogger<TableService> _log;
    private readonly TableConfig _config;
    private readonly RowClassGenerator _generator;
    private readonly TableStructureValidator _validator;

    public TableService(ILogger<TableService> log, TableConfig config, RowClassGenerator generator, TableStructureValidator validator)
    {
        _log = log;
        _config = config;
        _generator = generator;
        _validator = validator;
    }
    
    public bool Exists(string name, string schema)
    {
        return Directory.Exists(_config.TableDir(name, schema));
    }

    public CreateTableResult CreateIfNotExists(TableSpec spec)
    {
        var structureErrors = _validator.GetTableValidationErrors(spec);
        if (structureErrors != null)
            return new CreateTableResult
            {
                Errors = structureErrors.Errors
            };
        
        _log.LogInformation("No structure errors, generating source");
        var classFile = _generator.GetRowClassSource(spec);
        
        var assm = _generator.CreateAssembly(classFile, spec.Name);
        if (assm == null)
            return new CreateTableResult()
            {
                Errors = new List<string>
                {
                    "Compilation Failed"
                }
            };
        
        _log.LogInformation("Compilation Complete, setting up table");

        Directory.CreateDirectory(_config.TableDir(spec.Name, spec.Schema));
        File.WriteAllText(_config.TableSpecFile(spec.Name, spec.Schema),JsonSerializer.Serialize(spec));
        File.WriteAllText(_config.TableSourceFile(spec.Name, spec.Schema), classFile); 
        File.WriteAllBytes(_config.TableAssmFile(spec.Name, spec.Schema), assm);

        return new CreateTableResult();
    }

    public List<string> ListSchemas()
    {
        return Directory.EnumerateDirectories(_config.BaseSchemaDir)
            .Select(x => x.Split(Path.DirectorySeparatorChar).Last())
            .ToList();
    }
    
    
    public List<string> ListTables(string schema)
    {
        return Directory.EnumerateDirectories(_config.SchemaDir(schema))
            .Select(x => x.Split(Path.DirectorySeparatorChar).Last())
            .ToList();
    }

    public TableSpec? GetTableSpec(string name, string schema)
    {
        if (!Exists(name, schema))
            return null;
        
        var path = _config.TableSpecFile(name, schema);
        var text = File.ReadAllText(path);

        return JsonSerializer.Deserialize<TableSpec>(text);
    }

    public void DeleteTable(string schema, string name)
    {
        var path = _config.TableDir(name,schema);
        if(Directory.Exists(path))
            Directory.Delete(path);
    }
}