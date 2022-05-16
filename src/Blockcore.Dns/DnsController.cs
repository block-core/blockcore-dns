using DNS.Server;
using Microsoft.AspNetCore.Mvc;

namespace Blockcore.Dns
{
    [ApiController]
    [Route("api/dns")]
    public class DnsController : Controller
    {
        private readonly ILogger<DnsController> logger;

        public DnsController(ILogger<DnsController> logger, DnsMasterFile dnsMasterFile)
        {
            this.logger = logger;
            DnsMasterFile = dnsMasterFile;
        }

        public DnsMasterFile DnsMasterFile { get; }

        [HttpPost("addEntry")]
        public IActionResult AddEntry([FromBody] DnsRequest dnsRequest)
        {
            if (DnsMasterFile.TryAddIPAddressResourceRecord(dnsRequest))
            {
                logger.LogInformation($"Added entry {dnsRequest.Domain} - {dnsRequest.IpAddress} - {dnsRequest.Service} - {dnsRequest.Symbol}");
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
            return new OkObjectResult(DnsMasterFile.DnsEntries.Select(s => s.ToString()));
        }

        [HttpGet("ipaddress")]
        public IActionResult IpAddress()
        {
            return new OkObjectResult(Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? String.Empty);
        }
    }
}
