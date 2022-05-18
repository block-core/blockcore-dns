using DNS.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Blockcore.Dns
{
    [ApiController]
    [Route("api/dns")]
    public class DnsController : Controller
    {
        private readonly ILogger<DnsController> logger;

        public DnsController(ILogger<DnsController> logger, DomainService domainService, IdentityService identityService, IOptions<DnsSettings> options)
        {
            this.logger = logger; 
            DnsSettings = options.Value;
            DomainService = domainService;
            IdentityService = identityService;
        }

        public DomainService DomainService { get; }
        public IdentityService IdentityService { get; }
        public DnsSettings DnsSettings { get; }

        [HttpPost("addEntry")]
        public IActionResult AddEntry([FromBody] DnsRequest dnsRequest)
        {
            if (!IdentityService.VerifyIdentity(dnsRequest, DnsSettings))
            {
                return new StatusCodeResult(401);
            }

            DomainService.TryAddRecord(dnsRequest.Data);

            return new OkResult();
        }

        [HttpGet("entries")]
        public IActionResult Entries()
        {
            return new OkObjectResult(DomainService.DnsMasterFile.Entries.Select(s => s.ToString()));
        }

        [HttpGet("services")]
        public IActionResult ServiceEntries()
        {
            return new OkObjectResult(DomainService.DomainServiceEntries.Select(s => s.ToString()));
        }

        [HttpGet("ipaddress")]
        public IActionResult IpAddress()
        {
            return new OkObjectResult(Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? String.Empty);
        }
    }
}
