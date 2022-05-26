namespace Blockcore.Dns;

using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using System.Net;

public class DomainService : IDomainService
{
    private object locker;
    private IList<DomainServiceEntry> domainServiceEntries;
    private readonly ILogger<DomainService> logger;
    private IDnsMasterFile dnsMasterFile;

    public IList<DomainServiceEntry> DomainServiceEntries
    {
        get { return domainServiceEntries.ToList(); }
    }

    public IList<IResourceRecord> DnsServiceEntries
    {
        get { return dnsMasterFile.Entries.ToList(); }
    }

    public DomainService(ILogger<DomainService> logger, IDnsMasterFile dnsMasterFile)
    {
        this.logger = logger;
        this.dnsMasterFile = dnsMasterFile;
        locker = new object();
        domainServiceEntries = new List<DomainServiceEntry>();
    }

    public bool TryRemoveRecord(DomainServiceEntry serviceEntry)
    {
        lock (locker)
        {
            var newServiceEntries = domainServiceEntries.ToList();
            newServiceEntries.Remove(serviceEntry);
            domainServiceEntries = newServiceEntries;

            dnsMasterFile.TryRemoveIPAddressResourceRecord(serviceEntry.DnsRequest);
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

                    dnsMasterFile.TryAddOrUpdateIPAddressResourceRecord(dnsRequest);

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

                dnsMasterFile.TryAddOrUpdateIPAddressResourceRecord(dnsRequest);
            }

            logger.LogInformation($"Added entry = {dnsRequest}");

            return true;
        }
        else
        {
            // existing entry check if the domain has changed
            if (domain != null && (!domain.Equals(domainServiceEntry.Domain) || domainServiceEntry.DnsRequest.Ttl != dnsRequest.Ttl))
            {
                var oldEntry = domainServiceEntry.DnsRequest;
                domainServiceEntry.Domain = domain;
                domainServiceEntry.DnsRequest = dnsRequest;

                dnsMasterFile.TryRemoveIPAddressResourceRecord(oldEntry);
                dnsMasterFile.TryAddOrUpdateIPAddressResourceRecord(dnsRequest);

                logger.LogInformation($"Update entry = {dnsRequest}");

                return true;
            }
        }

        return false;
    }
}
