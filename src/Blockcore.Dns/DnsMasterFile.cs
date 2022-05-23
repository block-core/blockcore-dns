using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Server;
using Microsoft.Extensions.Options;
using System.Net;

namespace Blockcore.Dns
{
    public class DnsMasterFile : MasterFile, IDnsMasterFile
    {
        private object locker;

        public DnsMasterFile(IOptions<DnsSettings> options) : base(TimeSpan.FromMinutes(options.Value.DnsttlMinutes))
        {
            locker = new object();
        }

        public bool TryRemoveIPAddressResourceRecord(DnsData dnsRequest)
        {
            var record = new IPAddressResourceRecord(new Domain(dnsRequest.Domain), IPAddress.Parse(dnsRequest.IpAddress));
            var res = Get(record.Name, record.Type);
            foreach (var entry in res)
            {
                if (entry is IPAddressResourceRecord ipEntry)
                {
                    lock (locker)
                    {
                        var newEntries = entries.ToList();
                        newEntries.Remove(entry);
                        entries = newEntries;
                    }

                    return true;
                }
            }

            return false;
        }

        public bool TryAddOrUpdateIPAddressResourceRecord(DnsData dnsRequest)
        {
            if (string.IsNullOrEmpty(dnsRequest.Domain))
                return false;

            bool modified = false;
            var record = new IPAddressResourceRecord(new Domain(dnsRequest.Domain), IPAddress.Parse(dnsRequest.IpAddress));
            var res = Get(record.Name, record.Type);

            if (!res.Any())
            {
                lock (locker)
                {
                    var currentList = entries.ToList();
                    currentList.Add(record);
                    entries = currentList;
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

                        lock (locker)
                        {
                            var newEntries = entries.ToList();
                            newEntries.Remove(entry);
                            newEntries.Add(record);
                            entries = newEntries;
                        }

                        modified = true;
                    }
                }
            }

            return modified;
        }

        public IList<IResourceRecord> Entries { get { return entries.ToList(); } }
    }
}
