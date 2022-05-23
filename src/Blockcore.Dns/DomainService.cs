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
        private readonly ILogger<DomainService> logger;

        public IList<DomainServiceEntry> DomainServiceEntries { get { return domainServiceEntries.ToList(); } }
        public DnsMasterFile DnsMasterFile { get; }

        public DomainService(ILogger<DomainService> logger, DnsMasterFile dnsMasterFile)
        {
            this.logger = logger;
            DnsMasterFile = dnsMasterFile;
        }

        public bool TryRemoveRecord(DomainServiceEntry serviceEntry)
        {
            lock (locker)
            {
                var newServiceEntries = domainServiceEntries.ToList();
                newServiceEntries.Remove(serviceEntry);
                domainServiceEntries = newServiceEntries;

                DnsMasterFile.TryRemoveIPAddressResourceRecord(serviceEntry.DnsRequest);
            }

            logger.LogInformation($"Remove entry = {serviceEntry.DnsRequest}");

            return true;
        }

        public bool TryAddRecord(DnsData dnsRequest)
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

                        DnsMasterFile.TryAddOrUpdateIPAddressResourceRecord(dnsRequest);

                        logger.LogInformation($"Update entry = {dnsRequest}");

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

                    DnsMasterFile.TryAddOrUpdateIPAddressResourceRecord(dnsRequest);
                }

                logger.LogInformation($"Added entry = {dnsRequest}");

                return true;
            }
            else
            {
                // existing entry check if the domain has changed
                if (domain != null && !domain.Equals(domainServiceEntry.Domain))
                {
                    var oldEntry = domainServiceEntry.DnsRequest;
                    domainServiceEntry.Domain = domain;
                    domainServiceEntry.DnsRequest = dnsRequest;

                    DnsMasterFile.TryRemoveIPAddressResourceRecord(oldEntry);
                    DnsMasterFile.TryAddOrUpdateIPAddressResourceRecord(dnsRequest);

                    logger.LogInformation($"Update entry = {dnsRequest}");

                    return true;
                }
            }

            return false;
        }
    }
}
