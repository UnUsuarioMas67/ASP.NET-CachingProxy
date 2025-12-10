// See https://aka.ms/new-console-template for more information

using CachingProxyProject.Caching;
using CachingProxyProject.UI;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Development.json")
    .Build();

var redis = new RedisConnection(config);

var ui = new CachingProxyUI(redis);
ui.Run();