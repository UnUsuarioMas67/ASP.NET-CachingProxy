using System.Net;

namespace CachingProxyProject.Extensions;

public static class HttpListenerResponseExtensions
{
    public static async Task SetFromHttpResponseMessage(this HttpListenerResponse listenerResponse,
        HttpResponseMessage responseMessage)
    {
        listenerResponse.StatusCode = (int)responseMessage.StatusCode;
        listenerResponse.Headers.AddHeadersFrom(responseMessage.Headers);
        listenerResponse.AppendHeader("X-Cache", "miss");
        
        await listenerResponse.OutputStream.WriteAsync(await responseMessage.Content.ReadAsByteArrayAsync());
        listenerResponse.OutputStream.Close();
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
}