using DNS.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Blockcore.Dns;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {

        services.Configure<DnsSettings>(Configuration.GetSection("Dns"));

        services.AddResponseCompression();

        // Configure your services here
        services.AddSingleton<DnsMasterFile>();
        services.AddHostedService<DnsBackgroundService>();

        services.AddSwaggerGen(options =>
        {
            string assemblyVersion = typeof(Startup).Assembly.GetName().Version.ToString();

            options.SwaggerDoc("Dns", new OpenApiInfo
            {
                Title = "Blockcore Dns API",
                Version = assemblyVersion,
                Description = "Blockchain Dns used for blockchain based software and services.",
                Contact = new OpenApiContact
                {
                    Name = "Blockcore",
                    Url = new Uri("https://www.blockcore.net/")
                }
            });

            options.EnableAnnotations();
        });

        services.AddSwaggerGenNewtonsoftSupport(); // explicit opt-in - needs to be placed after AddSwaggerGen()

        services.AddControllers(options =>
        {
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseExceptionHandler("/error");

        // Enable Cors
        app.UseCors("IndexerPolicy");

        app.UseResponseCompression();

        //app.UseMvc();

        app.UseDefaultFiles();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseSwagger(c =>
        {
            c.RouteTemplate = "docs/{documentName}/openapi.json";

        });

        app.UseSwaggerUI(c =>
        {
           c.RoutePrefix = "docs";
           c.SwaggerEndpoint("/docs/Dns/openapi.json", "Blockcore Dns API");
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}