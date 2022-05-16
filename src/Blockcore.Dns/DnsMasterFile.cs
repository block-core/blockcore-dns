using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Server;
using System.Net;

namespace Blockcore.Dns
{
    public class DnsMasterFile: MasterFile
    {
        private object locker = new object();
        private IList<DnsEntry> dnsEntries = new List<DnsEntry>();

        public DnsMasterFile(TimeSpan ttl) : base(ttl)
        {
        }

        public DnsMasterFile() : base()
        {
        }

        public bool TryAddIPAddressResourceRecord(DnsRequest dnsRequest)
        {
            bool modified = false;
            var record = new IPAddressResourceRecord(new Domain(dnsRequest.Domain), IPAddress.Parse(dnsRequest.IpAddress));
            var newDnsEntry = new DnsEntry { DnsRequest = dnsRequest, Domain = record.Name, IpAddress = record.IPAddress };
            var res = Get(record.Name, record.Type);

            if (!res.Any())
            {
                lock (locker)
                {
                    var currentList = entries.ToList();
                    currentList.Add(record);
                    entries = currentList;

                    var currentDnsEntries = dnsEntries.ToList();
                    currentDnsEntries.Add(newDnsEntry);
                    dnsEntries = currentDnsEntries;
                }

                modified = true;
            }
            else
            {
                foreach (var entry in res)
                {
                    if (entry is IPAddressResourceRecord ipEntry)
                    {
                        if (ipEntry.IPAddress.Equals(record.IPAddress))
                        {
                            continue;
                        }

                        var dnsEntry = dnsEntries.Where(d => d.Domain == entry.Name).Single();

                        lock (locker)
                        {
                            var newEntries = entries.ToList();

                            newEntries.Remove(entry);
                            newEntries.Add(record);
                            entries = newEntries;

                            var newDnsEntries = dnsEntries.ToList();
                            newDnsEntries.Remove(dnsEntry);
                            newDnsEntries.Add(newDnsEntry);
                            dnsEntries = newDnsEntries;
                        }

                        modified = true;
                    }
                }
            }

            return modified;
        }

        public IList<IResourceRecord> Entries {  get { return entries.ToList(); } }
        public IList<DnsEntry> DnsEntries { get { return dnsEntries.ToList(); } }
    }
}
