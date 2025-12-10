using System.Net;
using CachingProxyProject.Caching;
using CachingProxyProject.Extensions;

namespace CachingProxyProject.Server;

public class ProxyServer(int port, string origin, RedisConnection redis)
{
    private HttpListener _listener;
    private RedisConnection _redis = redis;

    public int Port { get; } = port;
    public string Origin { get; } = origin;
    public string UriAddress => $"http://localhost:{Port}/";

    public void Start()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(UriAddress);
        _listener.Start();
    }

    public async Task Receive()
    {
        if (!_listener.IsListening)
            return;

        var context = await _listener.GetContextAsync();
        
        PrintHelper.PrintRequestUri(context.Request);
        
        var cacheResponse = await GetCacheResponseFromRedis(context);
        
        PrintHelper.PrintRequestHeaders(context.Request);
        
        if (cacheResponse != null)
        {
            await context.Response.SetFromCacheResponse(cacheResponse);
            PrintHelper.PrintResponseHeaders(context.Response);
        }
        else
        {
            var client = new HttpClient { BaseAddress = new Uri(Origin) };
            var request = context.Request.ToHttpRequestMessage();
            var response = await client.SendAsync(request);

            await context.Response.SetFromHttpResponseMessage(response);
            PrintHelper.PrintResponseHeaders(context.Response);
            
            await SaveCacheResponseToRedis(context, response.Content);
        }
        
        context.Response.OutputStream.Close();
        Console.WriteLine("\n\n");
    }

    public void Stop()
    {
        _listener.Stop();
    }

    private async Task<CacheResponse?> GetCacheResponseFromRedis(HttpListenerContext context)
    {
        // only allow get requests to be cached
        if (context.Request.HttpMethod != "GET")
        {
            Console.WriteLine("Caching is only allowed for GET requests\n");
            return null;
        }

        Console.WriteLine("Querying from cache...");
        
        var redisKey = RedisKey(context);
        var cacheResponse = await redis.GetJson<CacheResponse>(redisKey);

        Console.WriteLine((cacheResponse != null ? "Found in cache" : "Not in cache. Requesting from server...") + "\n");

        return cacheResponse;
    }

    private async Task SaveCacheResponseToRedis(HttpListenerContext context, HttpContent content)
    {
        // only allow get requests to be cached
        if (context.Request.HttpMethod != "GET")
            return;

        Console.WriteLine("Saving to cache...");
        
        var bytes = await content.ReadAsByteArrayAsync();
        var cacheResponse = context.Response.ToCacheResponse(bytes);
        
        var redisKey = RedisKey(context);
        var success = await redis.SaveAsJson(redisKey, cacheResponse);

        Console.WriteLine(success ? "Saved to cache successfully" : "Failed to save to cache");
    }

    private string RedisKey(HttpListenerContext context)
        => $"{Origin}{context.Request.RawUrl}";
}