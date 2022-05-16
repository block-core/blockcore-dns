using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Server;
using System.Net;

namespace Blockcore.Dns
{
    public class DnsMasterFile: MasterFile, IRequestResolver
    {
        private object locker = new object();

        public DnsMasterFile(TimeSpan ttl) : base(ttl)
        {
        }

        public DnsMasterFile() : base()
        {
        }

        public bool TryAddIPAddressResourceRecord(string domain, string ipAddress)
        {
            bool modified = false;
            var record = new IPAddressResourceRecord(new Domain(domain), IPAddress.Parse(ipAddress));
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
                            var currentList = entries.ToList();

                            currentList.Remove(entry);
                            currentList.Add(record);

                            entries = currentList;
                        }

                        modified = true;
                    }
                }
            }

            return modified;
        }

        public IList<IResourceRecord> DnsEntries {  get { return entries.ToList(); } }
    }
}
