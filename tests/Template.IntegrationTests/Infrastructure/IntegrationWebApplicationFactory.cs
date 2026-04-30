using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Template.WebApi;

namespace Template.IntegrationTests.Infrastructure;

public sealed class IntegrationWebApplicationFactory : WebApplicationFactory<IApiMarker>
{
    private readonly Dictionary<string, string?> _configurationOverrides = new(StringComparer.OrdinalIgnoreCase);
    private Action<IServiceCollection>? _configureTestServices;

    public IntegrationWebApplicationFactory WithConfigurationOverride(string key, string? value)
    {
        _configurationOverrides[key] = value;
        return this;
    }

    public IntegrationWebApplicationFactory WithTestServices(Action<IServiceCollection> configure)
    {
        _configureTestServices = _configureTestServices is null
            ? configure
            : services =>
            {
                _configureTestServices(services);
                configure(services);
            };

        return this;
    }

    public HttpClient CreateJsonClient()
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning);
        });

        builder.ConfigureAppConfiguration((_, config) =>
        {
            if (_configurationOverrides.Count > 0)
            {
                config.AddInMemoryCollection(_configurationOverrides);
            }
        });

        builder.ConfigureTestServices(services =>
        {
            // Ensure `UseHttpsRedirection()` won't unexpectedly redirect in TestServer.
            services.PostConfigure<HttpsRedirectionOptions>(o => o.HttpsPort = null);

            _configureTestServices?.Invoke(services);
        });
    }
}


