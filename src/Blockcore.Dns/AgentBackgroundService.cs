namespace Blockcore.Dns;
using Microsoft.Extensions.Options;

public class AgentBackgroundService : IHostedService, IDisposable
{
    private int executionCount = 0;
    private readonly ILogger<AgentBackgroundService> logger;
    private Timer timer = null!;
    public AgentSettings AgentSettings { get; }

    public AgentBackgroundService(ILogger<AgentBackgroundService> logger, IOptions<AgentSettings> options)
    {
        this.logger = logger;
        AgentSettings = options.Value;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"Timed Hosted Service running every {AgentSettings.IntervalMin} min.");

        timer = new Timer(DoWork, null, TimeSpan.Zero,TimeSpan.FromMinutes(AgentSettings.IntervalMin));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
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
