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

        [HttpGet("services")]
        public IActionResult ServiceEntries()
        {
            return new OkObjectResult(domainService.DomainServiceEntries.Select(s => s.ToString()));
        }

        [HttpGet("ipaddress")]
        public IActionResult IpAddress()
        {
            return new OkObjectResult(Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? String.Empty);
        }
    }
}
