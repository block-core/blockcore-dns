using DNS.Protocol.ResourceRecords;

namespace Blockcore.Dns
{
    public interface IDnsMasterFile
    {
        IList<IResourceRecord> Entries { get; }

        bool TryAddOrUpdateIPAddressResourceRecord(DnsData dnsRequest);
        bool TryRemoveIPAddressResourceRecord(DnsData dnsRequest);
    }
}