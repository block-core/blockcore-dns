namespace Blockcore.Dns
{
    public class AgentSettings
    {
        public string Secret { get; set; }
        public int IntervalMin { get; set; } = 5;
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
