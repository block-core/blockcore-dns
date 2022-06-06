using Microsoft.Extensions.Hosting;

namespace Blockcore.Dns;

using DNS.Client;
using DNS.Client.RequestResolver;
using DNS.Server;
using Microsoft.Extensions.Options;
using System.Text;

public class DnsBackgroundService : BackgroundService
{
    private readonly ILogger<DnsBackgroundService> logger;
    private DnsSettings dnsSettings;
    private IRequestResolver requestResolver;
    private DnsServer? dnsServer;

    public DnsBackgroundService(
        ILogger<DnsBackgroundService> logger,
        IRequestResolver requestResolver, 
        IOptions<DnsSettings> options)
    {
        this.logger = logger;
        this.requestResolver = requestResolver;
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
        dnsServer = string.IsNullOrEmpty(dnsSettings.EndServerIp)
            ? new DnsServer(requestResolver)
            : new DnsServer(requestResolver, dnsSettings.EndServerIp);

        dnsServer.Requested += (sender, e) =>
        {
            logger.LogDebug($"Dns Request = {e.Request}");
        };

        dnsServer.Responded += (sender, e) =>
        {
            logger.LogInformation($"{Environment.NewLine} Dns Request = {e.Request} {Environment.NewLine} Dns Response = {e.Response} {Environment.NewLine}");
        };

        // Log errors
        dnsServer.Errored += (sender, e) =>
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogError(e.Exception.ToString());

            }
            else
            {
                logger.LogError(e.Exception.Message);
            }
        };

        // Start the server (by default it listens on port 53)
        dnsServer.Listening += (sender, e) =>
        {
            logger.LogInformation($"Listening on port {dnsSettings.ListenPort}");

            if(!string.IsNullOrEmpty(dnsSettings.EndServerIp))
                logger.LogInformation($"End server ip =  {dnsSettings.EndServerIp}");
        };

        stoppingToken.Register(() =>
        {
            logger.LogInformation("Dns disposing");
            dnsServer.Dispose();
        });

        return dnsServer.Listen(dnsSettings.ListenPort);
    }
}