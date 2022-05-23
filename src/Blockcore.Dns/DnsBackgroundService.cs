using Microsoft.Extensions.Hosting;

namespace Blockcore.Dns;

using DNS.Client;
using DNS.Server;
using Microsoft.Extensions.Options;
using System.Text;

public class DnsBackgroundService : BackgroundService
{
    private readonly ILogger<DnsBackgroundService> logger;
    private DnsSettings dnsSettings;
    private IDnsMasterFile dnsMasterFile;
    private DnsServer? dnsServer;

    public DnsBackgroundService(
        ILogger<DnsBackgroundService> logger, 
        IDnsMasterFile dnsMasterFile, 
        IOptions<DnsSettings> options)
    {
        this.logger = logger;
        this.dnsMasterFile = dnsMasterFile;
        dnsSettings = options.Value;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine();

        foreach (string identity in dnsSettings.Identities)
        {
            stringBuilder.AppendLine(identity);
        }

        logger.LogInformation($"Configured identities : {stringBuilder}");

        logger.LogInformation($"Verify identities on registration : {dnsSettings.VerifyIdentity}.");

        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        dnsServer = new DnsServer(dnsMasterFile as DnsMasterFile, dnsSettings.EndServerIp);

        dnsServer.Requested += (sender, e) =>
        {
            logger.LogDebug($"Dns Request = {e.Request}");
        };

        dnsServer.Responded += (sender, e) =>
        {
            logger.LogInformation("{0} => {1}", e.Request, e.Response);
        };

        // Log errors
        dnsServer.Errored += (sender, e) =>
        {
            if (e.Exception.Message == "The operation was canceled.")
                return;

            logger.LogError(e.Exception.Message);
        };

        // Start the server (by default it listens on port 53)
        dnsServer.Listening += (sender, e) => logger.LogInformation($"Listening on port {dnsSettings.ListenPort}");

        stoppingToken.Register(() =>
        {
            logger.LogInformation("Dns disposing");
            dnsServer.Dispose();
        });

        return dnsServer.Listen(dnsSettings.ListenPort);
    }
}