using DNS.Protocol;
using System.Net;

namespace Blockcore.Dns
{
    public class DomainServiceEntry
    {
        public Domain? Domain { get; set; }
        public IPAddress IpAddress { get; set; }
        public DnsData DnsRequest { get; set; }

        public override string ToString()
        {
            return DnsRequest.ToString();
        }
    }
}
