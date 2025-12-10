using System.Collections.ObjectModel;
using System.Net;

namespace CachingProxyProject.Extensions;

public static class HttpListenerResponseExtensions
{
    public static async Task SetFromHttpResponseMessage(this HttpListenerResponse listenerResponse,
        HttpResponseMessage responseMessage)
    {
        listenerResponse.StatusCode = (int)responseMessage.StatusCode;
        listenerResponse.Headers.AddHeadersFrom(responseMessage.Headers);
        listenerResponse.ContentType = responseMessage.Content.Headers.ContentType?.MediaType;
        listenerResponse.AppendHeader("X-Cache", "MISS");
        await listenerResponse.OutputStream.WriteAsync(await responseMessage.Content.ReadAsByteArrayAsync());
    }
    
    public static CacheResponse ToCacheResponse(this HttpListenerResponse listenerResponse, byte[] content)
    {
        var headers = new List<KeyValuePair<string, string?>>();
        
        foreach (string headerName in listenerResponse.Headers.Keys)
            if (headerName != "X-Cache")
                headers.Add(new KeyValuePair<string, string?>(headerName, listenerResponse.Headers[headerName]));
        
        var cacheResponse = new CacheResponse
        {
            StatusCode = listenerResponse.StatusCode,
            ContentType = listenerResponse.ContentType,
            Headers = headers,
            Content = content
        };
        return cacheResponse;
    }

    public static async Task SetFromCacheResponse(this HttpListenerResponse listenerResponse, CacheResponse cacheResponse)
    {
        listenerResponse.StatusCode = cacheResponse.StatusCode;
        listenerResponse.ContentType = cacheResponse.ContentType;
        listenerResponse.Headers.AddHeadersFrom(cacheResponse.Headers);
        listenerResponse.AppendHeader("X-Cache", "HIT");
        await listenerResponse.OutputStream.WriteAsync(cacheResponse.Content);
    }

    private static void AddHeadersFrom(this WebHeaderCollection target,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> source)
    {
        foreach (var header in source)
        {
            if (target[header.Key] != null)
                target.Set(header.Key, string.Join(", ", header.Value));
            else
                target.Add(header.Key, string.Join(", ", header.Value));
        }
    }

    private static void AddHeadersFrom(this WebHeaderCollection target,
        IEnumerable<KeyValuePair<string, string?>> source)
    {
        foreach (var header in source)
        {
            if (target[header.Key] != null)
                target.Set(header.Key, header.Value);
            else
                target.Add(header.Key, header.Value);
        }
    }
}

public class CacheResponse
{
    public required int StatusCode { get; set; }
    public required List<KeyValuePair<string, string?>> Headers { get; set; }
    public required string? ContentType { get; set; }
    public required byte[] Content { get; set; }
}