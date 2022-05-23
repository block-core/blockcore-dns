using DNS.Protocol.Utils;

namespace Blockcore.Dns
{
    public class DnsRequest
    {
        public DnsAuth Auth { get; set; }
        public DnsData Data { get; set; }
    }
}
