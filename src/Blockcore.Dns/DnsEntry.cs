using DNS.Protocol;
using System.Net;

namespace Blockcore.Dns
{
    public class DnsEntry
    {
        public Domain Domain { get; set; }
        public IPAddress IpAddress { get; set; }
        public DnsRequest DnsRequest { get; set; }

        public override string ToString()
        {
            return DnsRequest.ToString();
        }
    }
}
