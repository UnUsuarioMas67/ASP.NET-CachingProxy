// See https://aka.ms/new-console-template for more information

using System.Net;
using CachingProxyProject;
using Cocona;


CoconaLiteApp.Run(async ([Option('p')] int port, [Option('o')] string origin) =>
{
    Console.WriteLine($"Starting server at port {port}...\nOrigin address: {origin}\n\n");

    var proxyServer = new ProxyServer(port, origin);
    proxyServer.Start();
    Console.CancelKeyPress += (sender, eventArgs) => { proxyServer.Stop(); };

    while (true)
    {
        try
        {
            await proxyServer.Receive();
        }
        catch (HttpListenerException)
        {
            break;
        }
    }

    Console.WriteLine("Server stopped");
});