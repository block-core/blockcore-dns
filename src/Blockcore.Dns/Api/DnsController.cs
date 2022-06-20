using Blockcore.Dns.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Blockcore.Dns.Api
{
    [ApiController]
    [Route("api/dns")]
    public class DnsController : Controller
    {
        private readonly ILogger<DnsController> logger;
        private IDomainService domainService;
        private IIdentityService identityService;
        private DnsSettings dnsSettings;

        public DnsController(
            ILogger<DnsController> logger, 
            IDomainService domainService, 
            IIdentityService identityService, 
            IOptions<DnsSettings> options)
        {
            this.logger = logger; 
            dnsSettings = options.Value;
            this.domainService = domainService;
            this.identityService = identityService;
        }


        [HttpPost("addEntry")]
        public IActionResult AddEntry([FromBody] DnsRequest dnsRequest)
        {
            if (!identityService.VerifyIdentity(dnsRequest, dnsSettings))
            {
                return new StatusCodeResult(401);
            }

            domainService.TryAddRecord(dnsRequest.Data);

            return new OkResult();
        }

        [HttpGet("entries")]
        public IActionResult Entries()
        {
            return new OkObjectResult(domainService.DnsServiceEntries.Select(s => s.ToString()));
        }

        [HttpGet("domains")]
        public IActionResult ServiceEntriesAll()
        {
            return new OkObjectResult(domainService.DomainServiceEntries.Select(s => s.ToString()));
        }

        [HttpGet("services")]
        public IActionResult ServiceEntries()
        {
            return new OkObjectResult(domainService.GetDomainData(null, null));
        }

        [HttpGet("services/symbol/{symbol}")]
        public IActionResult ServiceEntriesSymbols(string symbol)
        {
            return new OkObjectResult(domainService.GetDomainData(symbol, null));
        }

        [HttpGet("services/service/{service}")]
        public IActionResult ServiceEntriesService(string service)
        {
            return new OkObjectResult(domainService.GetDomainData(null, service));
        }

        [HttpGet("services/symbol/{symbol?}/service/{service?}")]
        public IActionResult ServiceEntries(string symbol, string service)
        {
            return new OkObjectResult(domainService.GetDomainData(symbol, service));
        }

        [HttpGet("ipaddress")]
        public IActionResult IpAddress()
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                foreach (var req in Request.Headers.ToList())
                {
                    builder.AppendLine($"{req.Key}, {req.Value}");
                }

                logger.LogDebug(builder.ToString());
            }

            return new OkObjectResult(Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? String.Empty);
        }
    }
}
