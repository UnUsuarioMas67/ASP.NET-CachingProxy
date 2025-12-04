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
        var request = CreateRequest(context);
        var response = await client.SendAsync(request);

        await SetListenerResponse(context, response);
    }

    public void Stop()
    {
        _listener.Stop();
    }

    private static HttpRequestMessage CreateRequest(HttpListenerContext context)
    {
        var method = new HttpMethod(context.Request.HttpMethod);
        var request = new HttpRequestMessage(method, context.Request.RawUrl);
        return request;
    }

    private static async Task SetListenerResponse(HttpListenerContext context, HttpResponseMessage response)
    {
        context.Response.StatusCode = (int)response.StatusCode;
        context.Response.ContentType = response.Content.Headers.ContentType?.MediaType;
        
        var content = await response.Content.ReadAsByteArrayAsync();

        await using var stream = context.Response.OutputStream;
        await stream.WriteAsync(content);

        context.Response.OutputStream.Close();
    }
}