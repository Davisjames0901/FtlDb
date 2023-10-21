using System.Text.Json;
using System.Text.Json.Nodes;
using AsperandLabs.FtlDb.Engine;
using AsperandLabs.FtlDb.Engine.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsperandLabs.FtlDb.TestServer.Controllers;

public class DataController : Controller
{
    private readonly DataService _dataService;
    private readonly TableService _tableService;

    public DataController(DataService dataService, TableService tableService)
    {
        _dataService = dataService;
        _tableService = tableService;
    }
    
    [HttpPut("{schema}/{table}/mount")]
    public IActionResult Mount(string table, string schema)
    {
        if (!_tableService.Exists(table, schema))
            return NotFound("Table does not exist");
        
        var mount = _dataService.TryMount(table, schema);
        if (mount == null)
            return StatusCode(500, "Failed to mount table");
        
        return Ok();
    }
    
    [HttpPut("{schema}/{table}/unmount")]
    public IActionResult Unmount(string table, string schema)
    {
        _dataService.Unmount(table, schema);
        return Ok();
    }
    
    
    [HttpPut("{schema}/{table}")]
    public IActionResult WriteRow(string table, string schema, [FromBody]JsonObject rowJson)
    {
        if (!_tableService.Exists(table, schema))
            return NotFound("Table does not exist");
        
        var mount = _dataService.TryMount(table, schema);
        if (mount == null)
            return StatusCode(500, "Failed to mount table");
        
        var rowType = mount.RowType();
        var row = rowJson.Deserialize(rowType);
        if (row == null)
            return BadRequest($"Could not parse key as {rowType}");
        
        var write = mount.WriteRow(row);
        if (write == null)
            return BadRequest("Failed to write row");
        return Ok(write);
    }
    
    [HttpPost("{schema}/{table}/match")]
    public IActionResult ReadRowByKey(string table, string schema, [FromBody]JsonObject payload)
    {
        if (!_tableService.Exists(table, schema))
            return NotFound("Table does not exist");
        
        var mount = _dataService.TryMount(table, schema);
        if (mount == null)
            return StatusCode(500, "Failed to mount table");

        var query = payload.ToList();
        if (query.Count == 0 || query.Count > 1)
            return BadRequest("One parameter is needed");

        var keyValue = query.First();
        var keyType = mount.GetIndexType(keyValue.Key);

        if (keyType == null)
            return BadRequest($"Index with name {keyValue.Key} does not exist");
        
        var key = keyValue.Value.Deserialize(keyType);
        if (key == null)
            return BadRequest($"Could not parse {keyValue.Key} as {keyType}");
        
        var read = mount.ReadFirstByIndex(keyValue.Key, key);
        if (read == null)
            return NotFound();
        return Ok(read);
    }
    
    [HttpGet("{schema}/{table}/count")]
    public IActionResult Count(string table, string schema)
    {
        if (!_tableService.Exists(table, schema))
            return NotFound("Table does not exist");
        
        var mount = _dataService.TryMount(table, schema);
        if (mount == null)
            return StatusCode(500, "Failed to mount table");
        
        return Ok(mount.Count());
    }
}