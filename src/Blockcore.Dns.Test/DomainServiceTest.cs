
using Blockcore.Dns.Agent;
using Blockcore.Dns.Api;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using Xunit;

namespace Blockcore.Dns.Test
{
    public class DomainServiceTest
    {
        static DnsData GenerateDnsData
        {
            get
            {
                return new DnsData
                {
                    Domain = "domain.com",
                    IpAddress = "1.1.1.1",
                    Port = 111,
                    Symbol = "BTC",
                    Service = "indexer"
                };
            }
        }

        [Fact]
        public void TryAddRecordSuccesTest()
        {
            DnsMasterFile dnsMasterFile = new DnsMasterFile(Options.Create(new DnsSettings()));
            DomainService service = new DomainService(new Mock<ILogger<DomainService>>().Object, dnsMasterFile);

            DnsData Data = GenerateDnsData;

            service.TryAddRecord(Data).Should().BeTrue();
            service.DomainServiceEntries.Should().HaveCount(1);
            service.DnsServiceEntries.Should().HaveCount(1);
        }

        [Fact]
        public void TryAddSameRecordSuccesTest()
        {
            DnsMasterFile dnsMasterFile = new DnsMasterFile(Options.Create(new DnsSettings()));
            DomainService service = new DomainService(new Mock<ILogger<DomainService>>().Object, dnsMasterFile);

            DnsData Data = GenerateDnsData;

            service.TryAddRecord(Data).Should().BeTrue();
            service.DomainServiceEntries.Should().HaveCount(1);
            service.DnsServiceEntries.Should().HaveCount(1);
            
            // add again
            service.TryAddRecord(Data).Should().BeFalse();
            service.DomainServiceEntries.Should().HaveCount(1);
            service.DnsServiceEntries.Should().HaveCount(1);
        }

        [Fact]
        public void TryAddSameRecordDifferentIpSuccesTest()
        {
            DnsMasterFile dnsMasterFile = new DnsMasterFile(Options.Create(new DnsSettings()));
            DomainService service = new DomainService(new Mock<ILogger<DomainService>>().Object, dnsMasterFile);

            DnsData Data = GenerateDnsData;

            service.TryAddRecord(Data).Should().BeTrue();
            service.DomainServiceEntries.Should().HaveCount(1);
            service.DnsServiceEntries.Should().HaveCount(1);

            DnsData Data1 = GenerateDnsData;
            Data1.IpAddress = "1.1.1.2";
            service.TryAddRecord(Data1).Should().BeTrue();
            service.DomainServiceEntries.Should().HaveCount(1);
            service.DnsServiceEntries.Should().HaveCount(1);
            service.DomainServiceEntries[0].IpAddress.Should().BeEquivalentTo(System.Net.IPAddress.Parse(Data1.IpAddress));
        }


        [Fact]
        public void TryAddDifferentRecordSuccesTest()
        {
            DnsMasterFile dnsMasterFile = new DnsMasterFile(Options.Create(new DnsSettings()));
            DomainService service = new DomainService(new Mock<ILogger<DomainService>>().Object, dnsMasterFile);

            DnsData Data = GenerateDnsData;

            service.TryAddRecord(Data).Should().BeTrue();
            service.DomainServiceEntries.Should().HaveCount(1);
            service.DnsServiceEntries.Should().HaveCount(1);

            DnsData Data1 = GenerateDnsData;
            Data1.Domain = "domain1.com";

            service.TryAddRecord(Data1).Should().BeTrue();
            service.DomainServiceEntries.Should().HaveCount(1);
            service.DnsServiceEntries.Should().HaveCount(1);

            // add again
            service.TryAddRecord(Data1).Should().BeFalse();
            service.DomainServiceEntries.Should().HaveCount(1);
            service.DnsServiceEntries.Should().HaveCount(1);
        }

        [Fact]
        public void TryAddMultipleDomainsSuccesTest()
        {
            DnsMasterFile dnsMasterFile = new DnsMasterFile(Options.Create(new DnsSettings()));
            DomainService service = new DomainService(new Mock<ILogger<DomainService>>().Object, dnsMasterFile);

            DnsData Data = GenerateDnsData;

            service.TryAddRecord(Data).Should().BeTrue();
            service.DomainServiceEntries.Should().HaveCount(1);
            service.DnsServiceEntries.Should().HaveCount(1);

            DnsData Data1 = GenerateDnsData;
            Data1.Domain = "domain1.com";
            Data1.IpAddress = "1.1.1.2";

            service.TryAddRecord(Data1).Should().BeTrue();
            service.DomainServiceEntries.Should().HaveCount(2);
            service.DnsServiceEntries.Should().HaveCount(2);

            DnsData Data2 = GenerateDnsData;
            Data2.Domain = "domain3.com";
            Data2.IpAddress = "1.1.1.3";

            service.TryAddRecord(Data2).Should().BeTrue();
            service.DomainServiceEntries.Should().HaveCount(3);
            service.DnsServiceEntries.Should().HaveCount(3);
        }

        [Fact]
        public void TryAddIpRecordSuccesTest()
        {
            DnsMasterFile dnsMasterFile = new DnsMasterFile(Options.Create(new DnsSettings()));
            DomainService service = new DomainService(new Mock<ILogger<DomainService>>().Object, dnsMasterFile);

            DnsData Data = GenerateDnsData;
            Data.Domain = null;

            service.TryAddRecord(Data).Should().BeTrue();
            service.DomainServiceEntries.Should().HaveCount(1);
            service.DnsServiceEntries.Should().HaveCount(0);
        }
    }
}