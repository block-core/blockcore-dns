namespace Blockcore.Dns;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBitcoin;
using NBitcoin.DataEncoders;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Contains("--help"))
        {
            Console.WriteLine($"The app has 3 modes:");
            Console.WriteLine($"--did mode to generate a did key pair");
            Console.WriteLine($"--agent mode to run as client that can register domains/IPs to a dns server (ddns)");
            Console.WriteLine($"otherwise to run as a dns server mode that can serve A/AAAA records and allow agents to register domains and IPs");
            return;
        }

        if (args.Contains("--did"))
        {
            // generating a DID key
            var key = new Key();
            var secret = key.GetBitcoinSecret(new DummyNetwork());
            string keyHex = Encoders.Hex.EncodeData(secret.ToBytes().Take(32).ToArray()); // bitcoin appends 1 byte
            string pubkeyHex = Encoders.Hex.EncodeData(key.PubKey.ToBytes());
            Console.WriteLine($"Secret key add to config {keyHex} ");
            Console.WriteLine($"Public identity did:is:{pubkeyHex} ");
            return;
        }

        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        // Run dns in agent mode
        if (args.Contains("--agent"))
        {
            return Host.CreateDefaultBuilder(args)
               .ConfigureServices((hostContext, services) =>
               {
                   StartupAgent.ConfigureAgent(hostContext, services);
               });
        }

        return Host.CreateDefaultBuilder(args)
           .ConfigureServices((hostContext, services) =>
           {
               services.Configure<HostOptions>(option => { });
           })
          .ConfigureAppConfiguration(config =>
          {
          })
          .ConfigureWebHostDefaults(webBuilder =>
          {
              webBuilder.ConfigureKestrel(serverOptions =>
              {
                  serverOptions.AddServerHeader = false;
              });

              webBuilder.UseStartup<Startup>();
          });
    }

    public class DummyNetwork : Networks.Network
    {
        public DummyNetwork()
        {
            this.Base58Prefixes = new byte[12][];
            this.Base58Prefixes[(int)Networks.Base58Type.SECRET_KEY] = new byte[] { (0) };
        }
    }
}