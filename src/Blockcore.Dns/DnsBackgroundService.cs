using Microsoft.Extensions.Hosting;

namespace Blockcore.Dns;

using DNS.Client;
using DNS.Server;
using Microsoft.Extensions.Options;

public class DnsBackgroundService : BackgroundService
{
    private readonly ILogger<DnsBackgroundService> logger;

    public DnsBackgroundService(ILogger<DnsBackgroundService> logger, DnsMasterFile dnsMasterFile, IOptions<DnsSettings> options)
    {
        this.logger = logger;
        DnsMasterFile = dnsMasterFile;
        DnsSettings = options.Value;
    }

    public DnsSettings DnsSettings { get; }

    public DnsMasterFile DnsMasterFile { get; }

    public DnsServer DnsServer { get; set; }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DnsServer = new DnsServer(DnsMasterFile, DnsSettings.EndServerIp);

        DnsServer.Requested += (sender, e) =>
        {
            logger.LogDebug($"Dns Request = {e.Request}");
        };

        DnsServer.Responded += (sender, e) =>
        {
            logger.LogInformation("{0} => {1}", e.Request, e.Response);
        };

        // Log errors
        DnsServer.Errored += (sender, e) =>
        {
            logger.LogError(e.Exception.Message);
        };

        // Start the server (by default it listens on port 53)
        DnsServer.Listening += (sender, e) => logger.LogInformation($"Listening on port {DnsSettings.ListenPort}");

        stoppingToken.Register(() =>
        {
            logger.LogInformation("Dns disposing");
            DnsServer.Dispose();
        });

        return DnsServer.Listen(DnsSettings.ListenPort);
    }
}