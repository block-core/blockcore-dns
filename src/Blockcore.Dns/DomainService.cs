using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Server;
using System.Net;

namespace Blockcore.Dns
{
    public class DomainService
    {
        private object locker = new object();
        private IList<DomainServiceEntry> domainServiceEntries = new List<DomainServiceEntry>();

        public IList<DomainServiceEntry> DomainServiceEntries { get { return domainServiceEntries.ToList(); } }

        public DomainService()
        {
        }

        public bool TryAddRecord(DnsRequest dnsRequest)
        {
            var ipAddress = IPAddress.Parse(dnsRequest.IpAddress);
            var domain = string.IsNullOrEmpty(dnsRequest.Domain) ? null : new Domain(dnsRequest.Domain);
            
            var domainServiceEntry = domainServiceEntries.Where(d => 
                d.IpAddress.Equals(ipAddress) && 
                dnsRequest.Port == d.DnsRequest.Port)
                .SingleOrDefault();

            if (domainServiceEntry == null)
            {
                // we try to find if this is a known domain where the ip address has changed
                if (domain != null)
                {
                    var domainDomainServiceEntry = domainServiceEntries.Where(d => d.Domain?.Equals(domain) ?? false).SingleOrDefault();

                    if (domainDomainServiceEntry != null)
                    {
                        domainDomainServiceEntry.IpAddress = ipAddress;
                        domainDomainServiceEntry.DnsRequest = dnsRequest;
                        return true;
                    }
                }

                // this is a new entry
                var newDnsServiceEntry = new DomainServiceEntry
                {
                    DnsRequest = dnsRequest,
                    Domain = domain,
                    IpAddress = ipAddress
                };

                lock (locker)
                {
                    var newServiceEntries = domainServiceEntries.ToList();
                    newServiceEntries.Add(newDnsServiceEntry);
                    domainServiceEntries = newServiceEntries;
                }

                return true;
            }
            else
            {
                // existing entry check if the domain has changed
                if (domain != null && !domain.Equals(domainServiceEntry.Domain))
                {
                    domainServiceEntry.Domain = domainServiceEntry.Domain;
                    domainServiceEntry.DnsRequest = dnsRequest;
                    return true;
                }
            }

            return false;
        }
    }
}
