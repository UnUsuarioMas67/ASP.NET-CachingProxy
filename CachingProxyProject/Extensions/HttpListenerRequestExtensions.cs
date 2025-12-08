using System.Net;

namespace CachingProxyProject.Extensions;

public static class HttpListenerRequestExtensions
{
    public static HttpRequestMessage ToHttpRequestMessage(this HttpListenerRequest listenerRequest)
    {
        var requestMessage = new HttpRequestMessage(new HttpMethod(listenerRequest.HttpMethod), listenerRequest.RawUrl);
        requestMessage.Content = listenerRequest.CopyStream();
        return requestMessage;
    }

    private static HttpContent? CopyStream(this HttpListenerRequest listenerRequest)
    {
        if (!listenerRequest.HasEntityBody) return null;
        
        var content = new StreamContent(listenerRequest.InputStream);
            
        // copy HttpListenerRequest content headers into HttpRequestMessage
        foreach (string headerName in listenerRequest.Headers.Keys)
        {
            if (headerName.StartsWith("Content-"))
                content.Headers.TryAddWithoutValidation(headerName, listenerRequest.Headers[headerName]);
        }

        return content;
    }
}