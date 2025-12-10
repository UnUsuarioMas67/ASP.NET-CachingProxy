using System.Net;
using CachingProxyProject.Extensions;

namespace CachingProxyProject;

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
        var cacheResponse = await GetCacheResponseFromRedis(context);

        if (cacheResponse != null)
        {
            await context.Response.SetFromCacheResponse(cacheResponse);
        }
        else
        {
            var client = new HttpClient { BaseAddress = new Uri(Origin) };
            var request = context.Request.ToHttpRequestMessage();
            var response = await client.SendAsync(request);

            await context.Response.SetFromHttpResponseMessage(response);
            await SaveCacheResponseToRedis(context, response.Content);
        }
        
        context.Response.OutputStream.Close();
        PrintRequest(context);
    }

    public void Stop()
    {
        _listener.Stop();
    }

    private async Task<CacheResponse?> GetCacheResponseFromRedis(HttpListenerContext context)
    {
        // only allow get requests to be cached
        if (context.Request.HttpMethod != "GET")
            return null;

        Console.WriteLine("Querying from cache...");
        
        var redisKey = RedisKey(context);
        var cacheResponse = await redis.GetJson<CacheResponse>(redisKey);

        Console.WriteLine(cacheResponse != null ? "Found in cache" : "Not in cache. Requesting from server...");

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

    #region Print Methods

    private static void PrintRequest(HttpListenerContext context)
    {
        Console.WriteLine($"{context.Request.HttpMethod} {context.Request.Url!.AbsoluteUri}");
        Console.WriteLine($"Status Code: {context.Response.StatusCode}\n");
        PrintRequestHeaders(context.Request);
        PrintResponseHeaders(context.Response);
        Console.WriteLine();
    }

    private static void PrintRequestHeaders(HttpListenerRequest request)
    {
        Console.WriteLine("---REQUEST HEADERS---");
        foreach (var header in request.Headers.AllKeys)
            Console.WriteLine($"{header}: {request.Headers[header]}");
        Console.WriteLine();
    }

    private static void PrintResponseHeaders(HttpListenerResponse response)
    {
        Console.WriteLine("---RESPONSE HEADERS---");
        foreach (var header in response.Headers.AllKeys)
            Console.WriteLine($"{header}: {response.Headers[header]}");
        Console.WriteLine();
    }

    #endregion
}