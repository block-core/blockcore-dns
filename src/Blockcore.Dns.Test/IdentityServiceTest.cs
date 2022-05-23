
using Blockcore.Dns.Agent;
using Blockcore.Dns.Api;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace Blockcore.Dns.Test
{
    public class IdentityServiceTest
    {
        [Fact]
        public void CreateAndVerifySignatureSuccesTest()
        {
            IdentityService identityService = new IdentityService(new Mock<ILogger<IdentityService>>().Object);

            DnsRequest dnsRequest = new DnsRequest
            {
                Data = new DnsData
                {
                    Domain = "domain.com",
                    IpAddress = "1.1.1.1",
                    Port = 1,
                    Symbol = "BTC",
                    Service = "indexer"
                }
            };

            AgentSettings agentSettings = new AgentSettings
            {
                Secret = "94589a0260a63b57a1a51ac5e06b8213b5d88503f67502f3324b926cb9e367e3"
            };

            identityService.CreateIdentity(dnsRequest, agentSettings);

            dnsRequest.Auth.Should().NotBeNull();
            dnsRequest.Auth.Signature.Should().NotBeNull();
            dnsRequest.Auth.Identity.Should().Be("did:is:03ba00574cc3821ae3d5f367696692e1b20ea25e70565e6fa6a07e7f74d266aa39");

            DnsSettings dnsSettings = new DnsSettings
            {
                Identities = new string[] { "did:is:03ba00574cc3821ae3d5f367696692e1b20ea25e70565e6fa6a07e7f74d266aa39" }
            };

            bool result = identityService.VerifyIdentity(dnsRequest, dnsSettings);

            result.Should().BeTrue();
        }

        [Fact]
        public void CreateAndVerifySignatureFaileTest()
        {
            IdentityService identityService = new IdentityService(new Mock<ILogger<IdentityService>>().Object);

            DnsRequest dnsRequest = new DnsRequest
            {
                Data = new DnsData
                {
                    Domain = "domain.com",
                    IpAddress = "1.1.1.1",
                    Port = 1,
                    Symbol = "BTC",
                    Service = "indexer"
                }
            };

            AgentSettings agentSettings = new AgentSettings
            {
                Secret = "94589a0260a63b57a1a51ac5e06b8213b5d88503f67502f3324b926cb9e367e3"
            };

            identityService.CreateIdentity(dnsRequest, agentSettings);

            dnsRequest.Auth.Should().NotBeNull();
            dnsRequest.Auth.Signature.Should().NotBeNull();
            dnsRequest.Auth.Identity.Should().Be("did:is:03ba00574cc3821ae3d5f367696692e1b20ea25e70565e6fa6a07e7f74d266aa39");

            // change something in the payload
            dnsRequest.Data.Domain = "domain1.com";

            DnsSettings dnsSettings = new DnsSettings
            {
                Identities = new string[] { "did:is:03ba00574cc3821ae3d5f367696692e1b20ea25e70565e6fa6a07e7f74d266aa39" }
            };

            bool result = identityService.VerifyIdentity(dnsRequest, dnsSettings);

            result.Should().BeFalse();
        }

        [Fact]
        public void CreateAndVerifySignatureInvalidTimeTest()
        {
            IdentityService identityService = new IdentityService(new Mock<ILogger<IdentityService>>().Object);

            DnsRequest dnsRequest = new DnsRequest
            {
                Data = new DnsData
                {
                    Domain = "domain.com",
                    IpAddress = "1.1.1.1",
                    Port = 1,
                    Symbol = "BTC",
                    Service = "indexer"
                }
            };

            AgentSettings agentSettings = new AgentSettings
            {
                Secret = "94589a0260a63b57a1a51ac5e06b8213b5d88503f67502f3324b926cb9e367e3"
            };

            identityService.CreateIdentity(dnsRequest, agentSettings);

            dnsRequest.Auth.Should().NotBeNull();
            dnsRequest.Auth.Signature.Should().NotBeNull();
            dnsRequest.Auth.Identity.Should().Be("did:is:03ba00574cc3821ae3d5f367696692e1b20ea25e70565e6fa6a07e7f74d266aa39");

            // change something in the payload
            dnsRequest.Data.Ticks = DateTime.UtcNow.AddDays(-1).Ticks;

            DnsSettings dnsSettings = new DnsSettings
            {
                Identities = new string[] { "did:is:03ba00574cc3821ae3d5f367696692e1b20ea25e70565e6fa6a07e7f74d266aa39" }
            };

            bool result = identityService.VerifyIdentity(dnsRequest, dnsSettings);

            result.Should().BeFalse();
        }
    }
}