using Microsoft.Extensions.DependencyInjection;

namespace Blockcore.Dns
{
    using NBitcoin;
    using NBitcoin.DataEncoders;
    using DNS.Client;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// The application program.
    /// </summary>
    public class Program
    {

        public static void Main(string[] args)
        {
            if (args.Contains("--did"))
            {
                // generating a DID key
                var key = new Key();
                var secret = key.GetBitcoinSecret(new DummyNetwork());
                string keyHex = Encoders.Hex.EncodeData(secret.ToBytes());
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
                       services.Configure<AgentSettings>(hostContext.Configuration.GetSection("DnsAgent"));
                       services.AddHostedService<AgentBackgroundService>();
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
}