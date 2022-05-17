namespace Blockcore.Dns;
using Microsoft.Extensions.Options;
using System.Net;

public class StatusBackgroundService : IHostedService, IDisposable
{
    private readonly ILogger<AgentBackgroundService> logger;
    private Timer timer = null!;
    private HttpClient httpClient;
    public DnsSettings DnsSettings { get; }

    public DnsMasterFile DnsMasterFile { get; }
    public DomainService DomainService { get; }

    public StatusBackgroundService(
        ILogger<AgentBackgroundService> logger, 
        IOptions<DnsSettings> options,
        DnsMasterFile dnsMasterFile, 
        DomainService domainService)
    {
        this.logger = logger;
        DnsSettings = options.Value;
        httpClient = new HttpClient();
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"Timed Hosted Service running every {DnsSettings.IntervalMin} min.");

        timer = new Timer(DoWork, null, TimeSpan.FromMinutes(DnsSettings.IntervalMin), TimeSpan.FromMinutes(DnsSettings.IntervalMin));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        if (!DomainService.DomainServiceEntries.Any())
        {
            logger.LogDebug($"No domain services found");
            return;
        }

        foreach (var serviceEntry in DomainService.DomainServiceEntries)
        {
            try
            {
                if (serviceEntry.Domain != null)
                {
                    if (serviceEntry.DnsRequest.Service == "indexer")
                    {
                        var responseMessage = httpClient.GetAsync($"https://{serviceEntry.Domain}/api/stats").Result;

                        if(!responseMessage.IsSuccessStatusCode)
                        {
                            DnsMasterFile.TryRemoveIPAddressResourceRecord(serviceEntry.DnsRequest);
                            DomainService.TryRemoveRecord(serviceEntry);
                        }

                        logger.LogInformation($"Removed service entry {serviceEntry.DnsRequest}.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Fail to fetc status info from host {serviceEntry.Domain} error= {ex.Message}");

                // todo: add failure counter threshold 
                // in case of failure we still remove the entries
                DnsMasterFile.TryRemoveIPAddressResourceRecord(serviceEntry.DnsRequest);
                DomainService.TryRemoveRecord(serviceEntry);
            }
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Timed Hosted Service is stopping.");

        timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        timer?.Dispose();
    }
}
