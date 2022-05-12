using DNS.Protocol.ResourceRecords;
using DNS.Server;

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

        public IList<IResourceRecord> DnsEntries {  get { return entries; } }
    }
}
