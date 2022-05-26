namespace Blockcore.Dns.Agent
{
    public class AgentSettings
    {
        public string Secret { get; set; }
        public int IntervalMinutes { get; set; } = 5;
        public int DnsttlMinutes { get; set; } = 20;
        public AgentHost[] Hosts { get; set; }
    }

    public class AgentHost
    {
        public string DnsHost { get; set; }
        public string Domain { get; set; }
        public int Port { get; set; }
        public string Symbol { get; set; }
        public string Service { get; set; }
    }
}
