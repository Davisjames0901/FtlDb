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
    
    [HttpPost("{schema}/{table}")]
    public IActionResult ReadRowByPrimaryKey(string table, string schema, [FromBody]JsonObject payload)
    {
        if (!_tableService.Exists(table, schema))
            return NotFound("Table does not exist");
        
        var mount = _dataService.TryMount(table, schema);
        if (mount == null)
            return StatusCode(500, "Failed to mount table");
        
        if (!payload.TryGetPropertyValue("key", out var keyJson))
            return BadRequest("Could not find key property");
        
        var keyType = mount.GetIndexType(null);

        var key = keyJson.Deserialize(keyType);
        if (key == null)
            return BadRequest($"Could not parse key as {keyType}");
        
        var read = mount.ReadByPrimaryKey(key);
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