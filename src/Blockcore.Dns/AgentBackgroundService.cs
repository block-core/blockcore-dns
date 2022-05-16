namespace Blockcore.Dns;
using Microsoft.Extensions.Options;
using System.Net;

public class AgentBackgroundService : IHostedService, IDisposable
{
    private readonly ILogger<AgentBackgroundService> logger;
    private Timer timer = null!;
    private HttpClient httpClient;
    public AgentSettings AgentSettings { get; }
    public IPAddress? ExternalIp { get; set; }


    public AgentBackgroundService(ILogger<AgentBackgroundService> logger, IOptions<AgentSettings> options)
    {
        this.logger = logger;
        AgentSettings = options.Value;
        httpClient = new HttpClient();
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"Timed Hosted Service running every {AgentSettings.IntervalMin} min.");

        timer = new Timer(DoWork, null, TimeSpan.Zero,TimeSpan.FromMinutes(AgentSettings.IntervalMin));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        if (!AgentSettings.Hosts.Any())
        {
            logger.LogWarning($"No hosts found, add hosts in the configuration file");
            return;
        }

        IPAddress? externalIp = null;
        foreach (var host in AgentSettings.Hosts)
        {
            if (externalIp == null)
            {
                try
                {
                    string externalIpString = httpClient.GetStringAsync($"http://{host.Host}/api/dns/ipaddress").Result;
                    externalIp = IPAddress.Parse(externalIpString.Replace("\\r\\n", "").Replace("\\n", "").Trim());

                    if (externalIp.IsIPv4MappedToIPv6)
                        externalIp = externalIp.MapToIPv4();

                    ExternalIp = externalIp;
                    logger.LogInformation($"Public ip = {externalIp}.");
                }
                catch (Exception ex)
                {
                    logger.LogError($"Fail to fetch external ip from host {host.Host} error= {ex}");
                    continue;
                }
            }

            if (externalIp != null)
            {
                try
                {
                    DnsRequest request = new DnsRequest
                    {
                        Domain = host.Domain,
                        IpAddress = externalIp.ToString()
                    };

                    var result = httpClient.PostAsJsonAsync($"http://{host.Host}/api/dns/addEntry", request).Result;

                    logger.LogInformation($"Updated host {host.Host} request {System.Text.Json.JsonSerializer.Serialize(request)}");

                }
                catch (Exception ex)
                {
                    logger.LogError($"Fail to post to server {host.Host} error= {ex}");
                }
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
