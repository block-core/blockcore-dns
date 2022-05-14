using Microsoft.Extensions.DependencyInjection;

namespace Blockcore.Dns
{
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
                string keyHex = "TBD";
                
                Console.WriteLine($"Add this DID key to config {keyHex} ");
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
    }
}