using Microsoft.Extensions.DependencyInjection;
using Template.Application.Constants;

namespace Template.IntegrationTests;

public class BaseApiTest : IAsyncLifetime
{
    public const int TestTenantId = 1;
    public const string TestUserName = "test-user";

    public IntegrationWebApplicationFactory AppFactory { get; }
    public HttpClient ApiClient { get; }
    public IServiceProvider Services { get; }

    protected BaseApiTest()
    {
        AppFactory = new IntegrationWebApplicationFactory(ConfigureServicesForTests);
        Services = AppFactory.Services;
        ApiClient = BuildClient();
    }

    public virtual async Task InitializeAsync()
    {
        await InternalInitializeAsync();
    }
    
    protected virtual Task InternalDisposeAsync() => Task.CompletedTask;
    protected virtual Task InternalInitializeAsync() => Task.CompletedTask;

    protected virtual void ConfigureServicesForTests(IServiceCollection services)
    { }
    
    public async Task DisposeAsync()
    {
        await InternalDisposeAsync();
        await AppFactory.DisposeAsync();
        ApiClient.Dispose();
    }

    /// <summary>
    /// Creates a raw HttpClient without tenant headers for testing middleware rejection.
    /// </summary>
    protected HttpClient BuildClientWithoutTenantHeaders()
    {
        return AppFactory.CreateClient();
    }

    private HttpClient BuildClient()
    {
        var client = AppFactory.CreateClient();
        client.DefaultRequestHeaders.Add(TenantHeaders.TenantId, $"{TestTenantId}");
        client.DefaultRequestHeaders.Add(TenantHeaders.UserName, TestUserName);
        return client;
    }
}