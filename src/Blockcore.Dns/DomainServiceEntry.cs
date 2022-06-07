using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using System.Net;

namespace Blockcore.Dns
{
    public class DomainServiceEntry
    {
        public Domain Domain { get; set; }
        public IPAddress IpAddress { get; set; }
        public DnsData DnsRequest { get; set; }
        public IPAddressResourceRecord IPAddressResourceRecord { get; set; }
        public CanonicalNameResourceRecord CNameResourceRecord { get; set; }
        public int FailedPings { get; set; }
        public DateTime LastSuccessPing { get; set; }

        public override string ToString()
        {
            return DnsRequest.Stringify().Add("FailedPings", FailedPings).Add("LastSuccessPing", LastSuccessPing).ToString();
        }

        public IEnumerable<IResourceRecord> Recoreds()
        {
            yield return IPAddressResourceRecord;

            if (CNameResourceRecord != null)
                yield return CNameResourceRecord;
        }
    }
}
