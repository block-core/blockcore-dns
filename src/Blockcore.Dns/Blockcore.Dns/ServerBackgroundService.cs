using Microsoft.Extensions.Hosting;

namespace Blockcore.Dns;

using DNS.Server;

public class ServerBackgroundService : BackgroundService
{
        

        public ServerBackgroundService()
        {
                

        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var masterFile = new MasterFile();
                // Resolve these domain to localhost
                masterFile.AddNameServerResourceRecord("DavidTestsForDns.Com.WeWork.com", "david.com");

                using var  server = new DnsServer(masterFile, "192.168.1.1");
                {
                    // Log every request
                    server.Requested += (sender, e) => Console.WriteLine(e.Request);
                    // On every successful request log the request and the response
                    server.Responded += (sender, e) => Console.WriteLine("{0} => {1}", e.Request, e.Response);
                    // Log errors
                    server.Errored += (sender, e) => Console.WriteLine(e.Exception.Message);
                    // Start the server (by default it listens on port 53)
                    
                    server.Listening += (sender, e) => Console.WriteLine("Listening");
                    
                    server.Listen();
                    
                    stoppingToken.WaitHandle.WaitOne();
                }
                
                return Task.CompletedTask;
        }
}