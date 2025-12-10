using System.Net;

namespace CachingProxyProject.Server;

public static class PrintHelper
{
    public static void PrintRequestUri(HttpListenerRequest request)
        => Console.WriteLine($"{request.HttpMethod} {request.Url!.AbsoluteUri}\n");

    public static void PrintRequestHeaders(HttpListenerRequest request)
    {
        Console.WriteLine("---REQUEST HEADERS---");
        foreach (var header in request.Headers.AllKeys)
            Console.WriteLine($"{header}: {request.Headers[header]}");
        Console.WriteLine();
    }

    public static void PrintResponseHeaders(HttpListenerResponse response)
    {
        Console.WriteLine("---RESPONSE HEADERS---");
        Console.WriteLine($"STATUS CODE: {response.StatusCode}\n");
        foreach (var header in response.Headers.AllKeys)
            Console.WriteLine($"{header}: {response.Headers[header]}");
        Console.WriteLine();
    }
}