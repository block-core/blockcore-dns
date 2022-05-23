using Blockcore.Dns.Agent;

namespace Blockcore.Dns
{
    public interface IIdentityService
    {
        void CreateIdentity(DnsRequest dnsRequest, AgentSettings agentSettings);
        string GetIdentity(AgentSettings agentSettings);
        bool VerifyIdentity(DnsRequest dnsRequest, DnsSettings dnsSettings);
    }
}