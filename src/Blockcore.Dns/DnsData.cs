using DNS.Protocol.Utils;

namespace Blockcore.Dns
{
    public class DnsData
    {
        public string Domain { get; set; }
        public string CNameDomain { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int SecurePort { get; set; }
        public string Symbol { get; set; }
        public string Service { get; set; }
        public int Ttl { get; set; }
        public long Ticks { get; set; }

        public override string ToString()
        {
            return ObjectStringifier.New(this).Add("Domain", "IpAddress", "Port", "SecurePort", "Symbol", "Service", "Ttl", "CNameDomain").ToString();
        }
    }
}
