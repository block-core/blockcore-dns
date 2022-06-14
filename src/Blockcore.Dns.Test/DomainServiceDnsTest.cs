using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using Xunit;

namespace Blockcore.Dns.Test
{
    public class DomainServiceDnsTest
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
                    Service = "indexer",
                    Ttl = 20
                };
            }
        }

        [Fact]
        public void TryReolveARecordSuccesTest()
        {
            DomainService service = new DomainService(new Mock<ILogger<DomainService>>().Object);

            DnsData Data = GenerateDnsData;

            service.TryAddRecord(Data).Should().BeTrue();

            Request request = new Request();
            request.Questions.Add(new Question(Domain.FromString(Data.Domain), RecordType.A));

            Response response = service.Resolve(request).Result as Response;

            response.ResponseCode.Should().Be(ResponseCode.NoError);
            response.AnswerRecords.Should().HaveCount(1);
            response.AnswerRecords[0].Name.Should().Be(Domain.FromString(Data.Domain));
            response.AnswerRecords[0].Should().BeOfType<IPAddressResourceRecord>();
            response.AnswerRecords[0].As<IPAddressResourceRecord>().IPAddress.Should().Be(IPAddress.Parse(Data.IpAddress));
        }

        [Fact]
        public void TryReolveTypeRecordNotFoundTest()
        {
            DomainService service = new DomainService(new Mock<ILogger<DomainService>>().Object);

            DnsData Data = GenerateDnsData;

            service.TryAddRecord(Data).Should().BeTrue();

            Request request = new Request();
            request.Questions.Add(new Question(Domain.FromString(Data.Domain), RecordType.CNAME));

            Response response = service.Resolve(request).Result as Response;

            response.AnswerRecords.Should().HaveCount(0);
            response.ResponseCode.Should().Be(ResponseCode.NameError);

        }

        [Fact]
        public void TryReolveARecordNotFoundTest()
        {
            DomainService service = new DomainService(new Mock<ILogger<DomainService>>().Object);

            DnsData Data = GenerateDnsData;

            service.TryAddRecord(Data).Should().BeTrue();

            Request request = new Request();
            request.Questions.Add(new Question(Domain.FromString("domain1.com"), RecordType.A));

            Response response = service.Resolve(request).Result as Response;

            response.AnswerRecords.Should().HaveCount(0);
            response.ResponseCode.Should().Be(ResponseCode.NameError);
        }

        [Fact]
        public void TryReolveNoPingFoundTest()
        {
            DomainService service = new DomainService(new Mock<ILogger<DomainService>>().Object);

            DnsData Data = GenerateDnsData;

            service.TryAddRecord(Data).Should().BeTrue();

            service.DomainServiceEntries[0].FailedPings = 1;

            Request request = new Request();
            request.Questions.Add(new Question(Domain.FromString(Data.Domain), RecordType.A));

            Response response = service.Resolve(request).Result as Response;

            response.ResponseCode.Should().Be(ResponseCode.NoError);
            response.AnswerRecords.Should().HaveCount(1);
            response.AnswerRecords[0].Name.Should().Be(Domain.FromString(Data.Domain));
            response.AnswerRecords[0].Should().BeOfType<IPAddressResourceRecord>();
            response.AnswerRecords[0].As<IPAddressResourceRecord>().IPAddress.Should().Be(IPAddress.Parse(Data.IpAddress));
        }

        [Fact]
        public void TryReolveCaaRecordTest()
        {
            DomainService service = new DomainService(new Mock<ILogger<DomainService>>().Object);

            DnsData Data = GenerateDnsData;

            service.TryAddRecord(Data).Should().BeTrue();

            Request request = new Request();
            request.Questions.Add(new Question(Domain.FromString("domain.com"), type: (RecordType)257));

            Response response = service.Resolve(request).Result as Response;

            response.AnswerRecords.Should().HaveCount(0);
            response.ResponseCode.Should().Be(ResponseCode.NoError);
        }
    }
}