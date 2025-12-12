using Microsoft.Extensions.Configuration;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

namespace CachingProxyProject.Caching;

public class RedisConnection
{
    private const string Prefix = "proxy:";
    
    private readonly IConfiguration _config;
    private readonly ConnectionMultiplexer _muxer;

    public RedisConnection(IConfiguration config)
    {
        _config = config.GetSection("Cache").GetSection("Redis");
        
        var endpoint = _config["ConnectionString"]!;
        var user = _config["UserName"]!;
        var password = _config["Password"];
        
        _muxer = ConnectionMultiplexer.Connect(new ConfigurationOptions
        {
            EndPoints = { endpoint },
            User = user,
            Password = password
        });
    }

    public async Task<bool> SaveAsJson(string key, object obj)
    {
        var db = _muxer.GetDatabase();
        
        var fullKey = $"{Prefix}{key}";
        var success = await db.JSON().SetAsync(fullKey, "$", obj);
        
        var expireMinutes = _config.GetValue<int>("ExpireMinutes");
        await db.KeyExpireAsync(fullKey, DateTime.UtcNow.AddMinutes(expireMinutes));
        return success;
    }

    public async Task<T?> GetJson<T>(string key)
        => await _muxer.GetDatabase().JSON().GetAsync<T>(Prefix + key);

    public async Task Clear()
    {
        var server = _muxer.GetServer(_muxer.GetEndPoints().First());
        var keys = server.Keys(pattern: $"{Prefix}*");
        var db = _muxer.GetDatabase();
        
        foreach (var key in keys)
            await db.KeyDeleteAsync(key);
    }
}