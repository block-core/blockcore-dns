using DNS.Protocol.Utils;

namespace Blockcore.Dns
{
    public class DnsResult
    {
        public string Domain { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int SecurePort { get; set; }
        public string Symbol { get; set; }
        public string Service { get; set; }
        public int Ttl { get; set; }
    }
}
