using Microsoft.Extensions.Configuration;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

namespace CachingProxyProject;

public class RedisConnection
{
    private readonly IConfiguration _config;
    private readonly IDatabase _db;

    public RedisConnection(IConfiguration config)
    {
        _config = config.GetSection("Cache").GetSection("Redis");
        
        var endpoint = _config["ConnectionString"]!;
        var user = _config["UserName"]!;
        var password = _config["Password"];
        
        var muxer = ConnectionMultiplexer.Connect(new ConfigurationOptions
        {
            EndPoints = { endpoint },
            User = user,
            Password = password
        });

        _db = muxer.GetDatabase();
    }

    public async Task<bool> SaveAsJson(string key, object obj)
    {
        var expireMinutes = _config.GetValue<int>("ExpireMinutes");
        
        var success = await _db.JSON().SetAsync(key, "$", obj);
        await _db.KeyExpireAsync(key, DateTime.UtcNow.AddMinutes(expireMinutes));
        return success;
    }

    public async Task<T?> GetJson<T>(string key)
        => await _db.JSON().GetAsync<T>(key);
}