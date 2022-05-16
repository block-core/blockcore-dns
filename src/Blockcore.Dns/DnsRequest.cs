using DNS.Protocol.Utils;

namespace Blockcore.Dns
{
    public class DnsRequest
    {
        public string Domain { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string Symbol { get; set; }
        public string Service { get; set; }

        public override string ToString()
        {
            return ObjectStringifier.New(this).Add("Domain", "IpAddress", "Port", "Symbol", "Service").ToString();
        }
    }
}
