using Blockcore.Dns.Identity;

namespace Blockcore.Dns.Agent;

public class StartupAgent
{
    public static void ConfigureAgent(HostBuilderContext hostContext, IServiceCollection services)
    {
        services.Configure<AgentSettings>(hostContext.Configuration.GetSection("DnsAgent"));
        services.AddSingleton<IIdentityService, IdentityService>();
        services.AddHostedService<AgentBackgroundService>();
    }
}