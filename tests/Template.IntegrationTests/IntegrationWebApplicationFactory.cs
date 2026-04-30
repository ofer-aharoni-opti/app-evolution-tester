using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.WebApi;
using Template.Testing.Common.Extensions;

namespace Template.IntegrationTests;

public class IntegrationWebApplicationFactory(Action<IServiceCollection>? servicesConfiguration = null)
    : WebApplicationFactory<IApiMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        
        foreach (var envVar in configuration.GetEnvironmentVariables())
        {
            Environment.SetEnvironmentVariable(envVar.Key, envVar.Value);
        }

        builder
            .UseConfiguration(configuration)
            .ConfigureAppConfiguration(c => c.AddConfiguration(configuration));
        
        builder.ConfigureServices(services =>
        {
            servicesConfiguration?.Invoke(services);
        });
    }
}