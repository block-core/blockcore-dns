namespace Blockcore.Dns
{
    public class DnsSettings
    {
        public int ListenPort { get; set; }
        public int IntervalMin { get; set; } = 5;
        public string EndServerIp { get; set; }
        public string[] Identities { get; set; }
    }
}
