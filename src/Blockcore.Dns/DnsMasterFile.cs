using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Server;
using System.Net;

namespace Blockcore.Dns
{
    public class DnsMasterFile: MasterFile
    {
        public DnsMasterFile(TimeSpan ttl) : base(ttl)
        {
        }

        public DnsMasterFile() : base()
        {
        }

        public bool CheckDomainExists(string domain, string ipAddress)
        {
            var record = new IPAddressResourceRecord(new Domain(domain), IPAddress.Parse(ipAddress));

            var res = Get(record.Name, record.Type);

            foreach (var entry in res)
            {
                if (entry is IPAddressResourceRecord ipEntry)
                {
                    if (ipEntry.IPAddress.Equals(record.IPAddress))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public IList<IResourceRecord> DnsEntries {  get { return entries; } }
    }
}
