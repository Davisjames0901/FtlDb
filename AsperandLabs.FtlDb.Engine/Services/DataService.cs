using AsperandLabs.FtlDb.Engine.Config;
using AsperandLabs.FtlDb.Engine.Row;
using AsperandLabs.FtlDb.Engine.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AsperandLabs.FtlDb.Engine;

public class DataService
{
    private readonly TableConfig _config;
    private readonly IServiceProvider _provider;
    private readonly TableService _tableService;
    private Dictionary<string, Mount> _mounts;
    private string Id(string table, string schema) => $"{schema}:{table}";

    public DataService(TableConfig config, IServiceProvider provider, TableService tableService)
    {
        _config = config;
        _provider = provider;
        _tableService = tableService;
        _mounts = new Dictionary<string, Mount>();
    }
    
    public Mount? TryMount(string table, string schema)
    {
        return GetMount(table, schema);
    }
    
    public void Unmount(string table, string schema)
    {
        var mountId = Id(table, schema);
        if (!_mounts.ContainsKey(mountId))
            return;

        var mount = _mounts[mountId];
        _mounts.Remove(mountId);
        mount.Dispose();
    }
    
    private Mount? GetMount(string table, string schema)
    {
        var mountId = Id(table, schema);
        if (_mounts.TryGetValue(mountId, out var mount))
            return mount;

        mount = _provider.GetService<Mount>()!;
        if (!mount.Init(table, schema))
            return null;
        
        _mounts.Add(mountId, mount);
        return mount;
    }
}