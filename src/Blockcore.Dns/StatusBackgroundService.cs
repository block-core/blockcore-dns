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
        DnsMasterFile = dnsMasterFile;
        DomainService = domainService;
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
                        DomainService.TryRemoveRecord(serviceEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Fail to fetc status info from host {serviceEntry.Domain} error= {ex.Message}");

                // todo: add failure counter threshold 
                // in case of failure we still remove the entries
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
