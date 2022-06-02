namespace Blockcore.Dns
{
    /// <summary>
    /// Settings for a DNS server.
    /// </summary>
    public class DnsSettings
    {
        /// <summary>
        /// DNS Port to listen for DNS queries (usually port 53) 
        /// </summary>
        public int ListenPort { get; set; } = 53;

        /// <summary>
        /// How often to check that registered services are on-line (if they are off line they get unregistered).
        /// </summary>
        public int IntervalMinutes { get; set; } = 5;

        /// <summary>
        /// A DNS server to call if a domain is not found on this DNS server, leave empty to ignore.
        /// Note: setting an ip address of a none DNS server may result in the blockcore dns timing out on queries.
        /// </summary>
        public string EndServerIp { get; set; }

        /// <summary>
        /// Identities in the form of DID (Decentralized Identifier) that are authorized to make registration requests.
        /// </summary>
        public string[] Identities { get; set; }

        /// <summary>
        /// A flag that specifies whether registration is required.
        /// </summary>
        public bool VerifyIdentity { get; set; } = true;
    }
}
