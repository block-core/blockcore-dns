namespace Blockcore.Dns;

using Blockcore.Dns.Identity;
using DNS.Client.RequestResolver;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OpenApi.Models;

public class StartupDns
{
    public IConfiguration Configuration { get; }

    public StartupDns(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<DnsSettings>(Configuration.GetSection("Dns"));

        services.AddResponseCompression();

        // Configure your services here
        services.AddSingleton<IIdentityService, IdentityService>();
        services.AddSingleton<DomainService>()
            .AddSingleton<IDomainService, DomainService>(provider => provider.GetService<DomainService>())
            .AddSingleton<IRequestResolver, DomainService>(provider => provider.GetService<DomainService>());
        services.AddHostedService<DnsBackgroundService>();
        services.AddHostedService<StatusBackgroundService>();

        services.AddSwaggerGen(options =>
        {
            string assemblyVersion = typeof(StartupDns).Assembly.GetName().Version.ToString();

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
            options.Conventions.Add(new ActionHidingConvention());
        });

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = null;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseForwardedHeaders();

        app.UseExceptionHandler("/error");
        app.UseCors("IndexerPolicy");
        app.UseResponseCompression();
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

    public class ActionHidingConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            // Replace with any logic you want
            if (!action.Controller.DisplayName.Contains("Blockcore.Dns"))
            {
                action.ApiExplorer.IsVisible = false;
            }
        }
    }
}