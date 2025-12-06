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

        await responseMessage.Content.CopyToAsync(listenerResponse.OutputStream);
        listenerResponse.OutputStream.Close();
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