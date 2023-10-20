using System.Diagnostics;
using AsperandLabs.FtlDb.Engine.Row;
using AsperandLabs.FtlDb.Engine.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsperandLabs.FtlDb.TestServer.Controllers;

[ApiController]
[Route("[controller]")]
public class TableController : Controller
{
    private readonly ILogger<TableController> _logger;
    private readonly TableService _tableService;

    public TableController(ILogger<TableController> logger, TableService tableService)
    {
        _logger = logger;
        _tableService = tableService;
    }

    [HttpPost("Create")]
    public IActionResult CreateTable(TableSpec request)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = _tableService.CreateIfNotExists(request);
        stopwatch.Stop();
        if (result.IsSuccessful)
            return Ok(stopwatch.ElapsedMilliseconds);
        return BadRequest(result.Errors);
    }
    
    [HttpGet("{schema}/{table}/exists")]
    public IActionResult Exists(string table, string schema = "dbo")
    {
        return Ok(_tableService.Exists(table, schema));
    }
    
    [HttpGet("{schema}/{table}")]
    public IActionResult Get(string table, string schema = "dbo")
    {
        return Ok(_tableService.GetTableSpec(table, schema));
    }
    
    [HttpDelete("{schema}/{table}")]
    public IActionResult Delete(string table, string schema = "dbo")
    {
        _tableService.DeleteTable(table, schema);
        return Ok();
    }
    
    [HttpGet("{schema}")]
    public IActionResult Exists(string schema = "dbo")
    {
        return Ok(_tableService.ListTables(schema));
    }
    
    [HttpGet("")]
    public IActionResult Exists()
    {
        return Ok(_tableService.ListSchemas());
    }
}