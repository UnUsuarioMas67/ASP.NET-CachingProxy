using System.Net;
using System.Text;

namespace CachingProxyProject;

public class ProxyServer(int port, string origin)
{
    private HttpListener _listener;

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

        Console.WriteLine($"{context.Request.HttpMethod} {context.Request.Url!.AbsoluteUri}");

        var client = new HttpClient { BaseAddress = new Uri(Origin) };
        var request = ListenerRequestToMessage(context.Request);
        var response = await client.SendAsync(request);
        
        await SetListenerResponseFromMessage(context.Response, response);
        
        PrintRequest(context);
    }

    public void Stop()
    {
        _listener.Stop();
    }

    private static HttpRequestMessage ListenerRequestToMessage(HttpListenerRequest request)
    {
        var method = new HttpMethod(request.HttpMethod);
        var requestMessage = new HttpRequestMessage(method, request.RawUrl);
        return requestMessage;
    }

    private static async Task SetListenerResponseFromMessage(
        HttpListenerResponse listenerResponse,
        HttpResponseMessage responseMessage)
    {
        listenerResponse.StatusCode = (int)responseMessage.StatusCode;
        listenerResponse.ContentType = responseMessage.Content.Headers.ContentType?.MediaType;

        var content = await responseMessage.Content.ReadAsByteArrayAsync();

        await using var stream = listenerResponse.OutputStream;
        await stream.WriteAsync(content);
    }
}