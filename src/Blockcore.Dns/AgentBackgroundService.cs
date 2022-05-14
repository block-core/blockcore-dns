namespace Blockcore.Dns;
using Microsoft.Extensions.Options;
using System.Net;

public class AgentBackgroundService : IHostedService, IDisposable
{
    private int executionCount = 0;
    private readonly ILogger<AgentBackgroundService> logger;
    private Timer timer = null!;
    private HttpClient httpClient;
    public AgentSettings AgentSettings { get; }

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
        try
        {
            // todo: add an endpoint on the dns api to return the callers ip
            // todo: change this to call an end point on the host (or one of the hosts if several are configured)
            string externalIpString = httpClient.GetStringAsync("http://icanhazip.com").Result.Replace("\\r\\n", "").Replace("\\n", "").Trim();
            var externalIp = IPAddress.Parse(externalIpString);

            logger.LogInformation($"Public ip = {externalIp}.");
        }
        catch (Exception ex)
        {
            logger.LogError($"Fail to fetch external ip {ex}");
            return;
        }

        var count = Interlocked.Increment(ref executionCount);

        logger.LogInformation("Timed Hosted Service is working. Count: {Count}", count);
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
