using DNS.Server;
using Microsoft.AspNetCore.Mvc;

namespace Blockcore.Dns
{
    [ApiController]
    [Route("api/dns")]
    public class DnsController : Controller
    {
        private readonly ILogger<DnsController> logger;

        public DnsController(ILogger<DnsController> logger, DnsMasterFile masterFile)
        {
            this.logger = logger;
            MasterFile = masterFile;
        }

        public DnsMasterFile MasterFile { get; }

        [HttpPost("addEntry")]
        public IActionResult AddEntry([FromBody] DnsRequest data)
        {
            MasterFile.AddIPAddressResourceRecord(data.Domain, data.IpAddress);
            
            logger.LogInformation($"Added entry {data.Domain} - {data.IpAddress}");

            return new OkResult();
        }

        [HttpGet("entries")]
        public IActionResult Entries()
        {
            return new OkObjectResult(MasterFile.DnsEntries.Select(s => s.ToString()));
        }

        [HttpGet("ipaddress")]
        public IActionResult IpAddress()
        {
            return new OkObjectResult(Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? String.Empty);
        }
    }
}
