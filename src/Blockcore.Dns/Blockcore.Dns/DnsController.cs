using DNS.Server;
using Microsoft.AspNetCore.Mvc;

namespace Blockcore.Dns
{
    [ApiController]
    [Route("api/dns")]
    public class DnsController : Controller
    {
        public DnsController(DnsMasterFile masterFile)
        {
            MasterFile = masterFile;
        }

        public DnsMasterFile MasterFile { get; }

        [HttpPost("send")]
        public IActionResult AddEntry([FromBody] DnsRequest data)
        {
            MasterFile.AddIPAddressResourceRecord(data.Domain, data.IpAddress);

            return new OkResult();
        }

        [HttpGet("entries")]
        public IActionResult Entries()
        {
            return new OkObjectResult(MasterFile.DnsEntries.Select(s => s.ToString()));
        }
    }
}
