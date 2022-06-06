namespace Blockcore.Dns;

using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class DomainService : IDomainService, IRequestResolver
{
    private object locker;
    private IDictionary<Domain, DomainServiceEntry> domainServiceEntries;
    private readonly ILogger<DomainService> logger;

    public IList<DomainServiceEntry> DomainServiceEntries
    {
        get 
        {
            var entries = domainServiceEntries;
            return entries.Values.ToList(); 
        }
    }

    public IList<IResourceRecord> DnsServiceEntries
    {
        get
        {
            var entries = domainServiceEntries;
            return entries.Values.SelectMany(s => s.Recoreds()).ToList();
        }
    }

    public DomainService(ILogger<DomainService> logger)
    {
        locker = new object();
        domainServiceEntries = new Dictionary<Domain, DomainServiceEntry>();
        this.logger = logger;
    }

    public Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default)
    {
        var entries = domainServiceEntries;

        IResponse response = Response.FromRequest(request);
        foreach (Question question in request.Questions)
        {
            // CAA records need to be answered with
            // NoError in order to work correctly

            if ((int)question.Type == 257) // 257 - CAA record type
            {
                IResponse caaResponse = Response.FromRequest(request);
                caaResponse.ResponseCode = ResponseCode.NoError;
                return Task.FromResult(caaResponse);
            }

            if (entries.TryGetValue(question.Name, out DomainServiceEntry? domainServiceEntry))
            {

                if (domainServiceEntry.FailedPings == 0)
                {
                    var records = question.Type == RecordType.ANY ?
                        domainServiceEntry.Recoreds() :
                        domainServiceEntry.Recoreds().Where(w => w.Type == question.Type);

                    foreach (var record in records)
                    {
                        response.AnswerRecords.Add(record);
                    }
                }
            }
        }

        if (response.AnswerRecords.Count == 0)
        {
            response.ResponseCode = ResponseCode.NameError;
        }

        return Task.FromResult(response);
    }

    public bool TryRemoveRecord(DomainServiceEntry serviceEntry)
    {
        lock (locker)
        {
            if (domainServiceEntries.ContainsKey(serviceEntry.Domain))
            {
                var entries = domainServiceEntries.ToDictionary(d => d.Key, d => d.Value);
                if (entries.Remove(serviceEntry.Domain))
                {
                    domainServiceEntries = entries;
                    logger.LogInformation($"Remove entry = {serviceEntry.DnsRequest}");
                }
            }
        }

        return true;
    }

    public bool TryAddRecord(DnsData dnsRequest)
    {
        var ipAddress = IPAddress.Parse(dnsRequest.IpAddress);
        var domain = Domain.FromString(dnsRequest.Domain);

        lock (locker)
        {
            if (domainServiceEntries.TryGetValue(domain, out DomainServiceEntry? domainServiceEntry))
            {
                // this is an existing entry only modify
                // if any of the properties have changed
                // no need to make changes to the dictionary

                bool modified = !domainServiceEntry.IpAddress.Equals(ipAddress)
                    || domainServiceEntry.DnsRequest.CNameDomain != dnsRequest.CNameDomain
                    || domainServiceEntry.DnsRequest.Ttl != dnsRequest.Ttl
                    || domainServiceEntry.DnsRequest.Port != dnsRequest.Port;

                if (modified)
                {
                    PopulateEntry(domainServiceEntry, dnsRequest);
                    logger.LogInformation($"Update entry = {dnsRequest}");
                    return true;
                }

                return false;
            }

            // this is a new entry
            var newDnsServiceEntry = new DomainServiceEntry { Domain = domain };
            PopulateEntry(newDnsServiceEntry, dnsRequest);

            var entries = domainServiceEntries.ToDictionary(d => d.Key, d => d.Value);
            if (entries.TryAdd(newDnsServiceEntry.Domain, newDnsServiceEntry))
            {
                domainServiceEntries = entries;
                logger.LogInformation($"Added entry = {dnsRequest}");
            }
        }

        return true;
    }

    private void PopulateEntry(DomainServiceEntry domainServiceEntry, DnsData dnsRequest)
    {
        var ipAddress = IPAddress.Parse(dnsRequest.IpAddress);
        var domain = Domain.FromString(dnsRequest.Domain);

        domainServiceEntry.IpAddress = ipAddress;
        domainServiceEntry.DnsRequest = dnsRequest;
        domainServiceEntry.IPAddressResourceRecord = new IPAddressResourceRecord(domain, ipAddress, TimeSpan.FromMinutes(dnsRequest.Ttl));

        if (dnsRequest.CNameDomain != null)
        {
            var cnameDomain = new Domain(dnsRequest.CNameDomain);
            domainServiceEntry.CNameResourceRecord = new CanonicalNameResourceRecord(domain, cnameDomain, TimeSpan.FromMinutes(dnsRequest.Ttl));
        }
    }
}
