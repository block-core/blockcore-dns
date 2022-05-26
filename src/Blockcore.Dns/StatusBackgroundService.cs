namespace Blockcore.Dns;
using Microsoft.Extensions.Options;

/// <summary>
/// A background service that will iterate over all the registered domains
/// (or if the domain is null it will try the ip address) and check if the domain is still online.
/// If the domain is not online it will get unregistered.
/// </summary>
public class StatusBackgroundService : IHostedService, IDisposable
{
    private readonly ILogger<StatusBackgroundService> logger;
    private Timer timer;
    private readonly HttpClient httpClient;
    private readonly DnsSettings dnsSettings;
    private readonly IDomainService domainService;

    public StatusBackgroundService(
        ILogger<StatusBackgroundService> logger, 
        IOptions<DnsSettings> options,
        IDomainService domainService)
    {
        timer = null!;
        this.logger = logger;
        this.domainService = domainService;
        dnsSettings = options.Value;
        httpClient = new HttpClient();
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"Timed Hosted Service running every {dnsSettings.IntervalMinutes} min.");

        timer = new Timer(DoWork, null, TimeSpan.FromMinutes(dnsSettings.IntervalMinutes), TimeSpan.FromMinutes(dnsSettings.IntervalMinutes));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        var domainEntrie = domainService.DomainServiceEntries;

        if (!domainEntrie.Any())
        {
            logger.LogDebug($"No domain services found");
            return;
        }

        foreach (var serviceEntry in domainEntrie)
        {
            try
            {
                if (serviceEntry.DnsRequest.Service.ToLower() == "indexer")
                {
                    string? url = serviceEntry.Domain != null ? $"https://{serviceEntry.Domain}/api/stats" 
                                : serviceEntry.IpAddress != null ? $"http://{serviceEntry.IpAddress}:{serviceEntry.DnsRequest.Port}/api/stats" 
                                : null;

                    if (url == null)
                        continue;

                    var responseMessage = httpClient.GetAsync(url).Result;

                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        domainService.TryRemoveRecord(serviceEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Fail to fetc status info from host {serviceEntry.Domain} error= {ex.Message}");

                // todo: add failure counter threshold 
                // in case of failure we still remove the entries
                domainService.TryRemoveRecord(serviceEntry);
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
