namespace Blockcore.Dns.Agent
{
    /// <summary>
    /// Setting for a DNS agent.
    /// </summary>
    public class AgentSettings
    {
        /// <summary>
        /// A private key that will be used as a DID to sign requests to the DNS server.
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// How often to notify the DNS server about our IP address.
        /// </summary>
        public int IntervalMinutes { get; set; } = 5;

        /// <summary>
        /// The Time to Live use by DNS server to cache IP addresses.
        /// </summary>
        public int DnsttlMinutes { get; set; } = 20;

        /// <summary>
        /// Domain data representing services that we want the DNS agent to register with the DNS server.
        /// </summary>
        public AgentHost[] Hosts { get; set; }
    }

    /// <summary>
    /// Meta data that represents local running services and the DNS server we wish to register them with.
    /// </summary>
    public class AgentHost
    {
        /// <summary>
        /// The DNS server to register domain data with.
        /// </summary>
        public string DnsHost { get; set; }

        /// <summary>
        /// The domain of the local service.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// A canonical domain to redirect callers in case the service is off line.
        /// </summary>
        public string CNameDomain { get; set; }

        /// <summary>
        /// The local port the service is running under (to allow access via ip/port not just domain).
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The local secure (ssl) port the service is running under (to allow access via ip/port not just domain).
        /// </summary>
        public int SecurePort { get; set; }

        /// <summary>
        /// The coin symbol this service represents.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// The type of service that is represented locally.
        /// </summary>
        public string Service { get; set; }
    }
}
