using Microsoft.Extensions.Hosting;

namespace Blockcore.Dns;

using DNS.Client;
using DNS.Server;
using Microsoft.Extensions.Options;

public class DnsBackgroundService : BackgroundService
{
    public DnsBackgroundService(DnsMasterFile masterFile, IOptions<DnsSettings> options)
    {
        MasterFile = masterFile;
        DnsSettings = options.Value;
    }

    public DnsSettings DnsSettings { get; }

    public DnsMasterFile MasterFile { get; }

    public DnsServer DnsServer { get; set; }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DnsServer = new DnsServer(MasterFile, DnsSettings.EndServerIp);

        DnsServer.Requested += (sender, e) =>
        {

            Console.WriteLine(e.Request);
        };

        DnsServer.Responded += (sender, e) =>
        {
            Console.WriteLine("{0} => {1}", e.Request, e.Response);
        };

        // Log errors
        DnsServer.Errored += (sender, e) =>
        {
            Console.WriteLine(e.Exception.Message);
        };

        // Start the server (by default it listens on port 53)
        DnsServer.Listening += (sender, e) => Console.WriteLine("Listening");

        //server.Listening += async (sender, e) =>
        //{
        //    DnsClient client = new DnsClient("127.0.0.1", PORT);

        //    //var res1 = await client.Lookup("google.com").ConfigureAwait(false);
        //    //var res2 = await client.Lookup("cnn.com").ConfigureAwait(false);

        //    var res3 = await client.Resolve("ns1.DavidTestsForDns.Com.WeWork.com", DNS.Protocol.RecordType.NS).ConfigureAwait(false);


        //    server.Dispose();
        //};

        stoppingToken.Register(DnsServer.Dispose);

        return DnsServer.Listen(DnsSettings.ListenPort);
    }
}