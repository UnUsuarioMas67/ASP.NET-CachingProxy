using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
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
        var client = new HttpClient { BaseAddress = new Uri(Origin) };
        var request = context.Request.ToHttpRequestMessage();
        var response = await client.SendAsync(request);
        
        await context.Response.SetFromHttpResponseMessage(response);
        
        PrintRequest(context);
    }

    public void Stop()
    {
        _listener.Stop();
    }

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