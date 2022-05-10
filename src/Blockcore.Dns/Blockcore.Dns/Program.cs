// Proxy to google's DNS
//


using Blockcore.Dns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host =  Host.CreateDefaultBuilder(args)
    .UseConsoleLifetime()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<ServerBackgroundService>();
    })
    .Build();

await host.RunAsync();