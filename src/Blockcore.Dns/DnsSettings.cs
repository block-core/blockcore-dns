namespace Blockcore.Dns
{
    public class DnsSettings
    {
        public int ListenPort { get; set; } = 53;
        public int IntervalMinutes { get; set; } = 5;
        public string EndServerIp { get; set; }
        public string[] Identities { get; set; }
        public bool VerifyIdentity { get; set; } = true;
    }
}
