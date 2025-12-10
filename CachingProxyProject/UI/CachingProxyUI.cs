using System.Net;
using CachingProxyProject.Caching;
using CachingProxyProject.Server;
using Cocona;
using Microsoft.Extensions.Configuration;

namespace CachingProxyProject.UI;

public class CachingProxyUI(RedisConnection redis)
{
    public void Run()
    {
        CoconaLiteApp.Run(async ([Option('p')] int port, [Option('o')] string origin) =>
        {
            Console.WriteLine($"Starting server at port {port}...\nOrigin address: {origin}\n\n");

            var proxyServer = new ProxyServer(port, origin, redis);
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
    }
}