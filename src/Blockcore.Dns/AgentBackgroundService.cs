namespace Blockcore.Dns;
using Microsoft.Extensions.Options;
using System.Net;

/// <summary>
/// A background service that will resolve the agnets public ip address and 
/// register domain entries with the blockcore-dns server.
/// </summary>
public class AgentBackgroundService : IHostedService, IDisposable
{
    private readonly ILogger<AgentBackgroundService> logger;
    private Timer timer;
    private HttpClient httpClient;
    private AgentSettings agentSettings;
    private IIdentityService identityService;

    public IPAddress? ExternalIp { get; set; }

    public AgentBackgroundService(
        ILogger<AgentBackgroundService> logger, 
        IIdentityService identityService, 
        IOptions<AgentSettings> options)
    {
        timer = null!;
        this.logger = logger;
        this.identityService = identityService;
        agentSettings = options.Value;
        httpClient = new HttpClient();
    }


    public Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"Configured identity = {identityService.GetIdentity(agentSettings)}.");

        logger.LogInformation($"Timed Hosted Service running every {agentSettings.IntervalMin} min.");

        timer = new Timer(DoWork, null, TimeSpan.Zero,TimeSpan.FromMinutes(agentSettings.IntervalMin));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        if (!agentSettings.Hosts.Any())
        {
            logger.LogWarning($"No hosts found, add hosts in the configuration file");
            return;
        }

        IPAddress? externalIp = null;
        foreach (var host in agentSettings.Hosts)
        {
            // use one of the blockcore-dns hosts to revolve
            // the public ip address for the agent.

            if (externalIp == null)
            {
                try
                {
                    string externalIpString = httpClient.GetStringAsync($"http://{host.DnsHost}/api/dns/ipaddress").Result;
                    externalIp = IPAddress.Parse(externalIpString.Replace("\\r\\n", "").Replace("\\n", "").Trim());

                    if (externalIp.IsIPv4MappedToIPv6)
                        externalIp = externalIp.MapToIPv4();

                    ExternalIp = externalIp;
                    logger.LogInformation($"Public ip = {externalIp}.");
                }
                catch (Exception ex)
                {
                    logger.LogError($"Fail to fetch external ip from host {host.DnsHost} error= {ex}");
                    continue;
                }
            }

            if (externalIp != null)
            {
                try
                {
                    DnsRequest request = new DnsRequest
                    {
                        Data = new DnsData
                        {
                            Domain = host.Domain,
                            IpAddress = externalIp.ToString(),
                            Port = host.Port,
                            Service = host.Service,
                            Symbol = host.Symbol
                        }
                    };

                    identityService.CreateIdentity(request, agentSettings);

                    var result = httpClient.PostAsJsonAsync($"http://{host.DnsHost}/api/dns/addEntry", request).Result;

                    logger.LogInformation($"Updated host {host.DnsHost} request {System.Text.Json.JsonSerializer.Serialize(request)}");

                }
                catch (Exception ex)
                {
                    logger.LogError($"Fail to post to server {host.DnsHost} error= {ex}");
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
