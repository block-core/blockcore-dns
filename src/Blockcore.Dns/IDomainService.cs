
using DNS.Protocol.ResourceRecords;

namespace Blockcore.Dns
{
    public interface IDomainService
    {
        IList<DomainServiceEntry> DomainServiceEntries { get; }
        IList<IResourceRecord> DnsServiceEntries { get; }

        bool TryAddRecord(DnsData dnsRequest);
        bool TryRemoveRecord(DomainServiceEntry serviceEntry);

        IList<DnsResult> GetDomainData(string? symbol, string? service);
    }
}