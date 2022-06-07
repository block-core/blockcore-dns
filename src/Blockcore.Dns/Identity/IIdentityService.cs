using Blockcore.Dns.Agent;
using Blockcore.Dns.Api;

namespace Blockcore.Dns.Identity
{
    public interface IIdentityService
    {
        void CreateIdentity(DnsRequest dnsRequest, AgentSettings agentSettings);
        string GetIdentity(AgentSettings agentSettings);
        bool VerifyIdentity(DnsRequest dnsRequest, DnsSettings dnsSettings);
    }
}