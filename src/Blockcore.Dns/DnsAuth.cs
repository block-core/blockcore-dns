using DNS.Protocol.Utils;

namespace Blockcore.Dns
{
    public class DnsAuth
    {
        public string Identity { get; set; }
        public string Signature { get; set; }
    }
}
