using DNS.Server;
using Microsoft.AspNetCore.Mvc;

namespace Blockcore.Dns
{
    [ApiController]
    [Route("api/dns")]
    public class DnsController : Controller
    {
        private readonly ILogger<DnsController> logger;

        public DnsController(ILogger<DnsController> logger, DnsMasterFile dnsMasterFile, DomainService domainService)
        {
            this.logger = logger;
            DnsMasterFile = dnsMasterFile;
            DomainService = domainService;
        }

        public DnsMasterFile DnsMasterFile { get; }
        public DomainService DomainService { get; }

        [HttpPost("addEntry")]
        public IActionResult AddEntry([FromBody] DnsRequest dnsRequest)
        {
            var dnsAdded = DnsMasterFile.TryAddOrUpdateIPAddressResourceRecord(dnsRequest);
            var serviceAdded = DomainService.TryAddRecord(dnsRequest);

            if(dnsAdded || serviceAdded)
            {
                logger.LogInformation($"Added service={serviceAdded} dns={dnsAdded} entry= {dnsRequest.Domain} - {dnsRequest.IpAddress} - {dnsRequest.Service} - {dnsRequest.Symbol}");
            }

            return new OkResult();
        }

        [HttpGet("entries")]
        public IActionResult Entries()
        {
            return new OkObjectResult(DnsMasterFile.Entries.Select(s => s.ToString()));
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
