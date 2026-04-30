using System.Net;
using System.Net.Http.Json;
using Template.Application.Features.Test;
using Template.WebApi.Contracts.Test;

namespace Template.IntegrationTests.Tests;

public sealed class TestEndpointsTests : BaseApiTest
{
    [Fact]
    public async Task Get_test_returns_echo()
    {
        var response = await ApiClient.GetFromJsonAsync<GetTest.Response>("/api/v1/test?value=hello");
        Assert.NotNull(response);
        Assert.Equal("hello", response.Echo);
    }

    [Fact]
    public async Task Get_test_without_value_returns_400()
    {
        using var response = await ApiClient.GetAsync("/api/v1/test");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_v1_test_returns_message_with_processed_at()
    {
        using var httpResponse = await ApiClient.PostAsJsonAsync(
            "/api/v1/test",
            new CreateTestRequest("abc"));

        httpResponse.EnsureSuccessStatusCode();
        var response = await httpResponse.Content.ReadFromJsonAsync<CreateTestResponse>();
        Assert.NotNull(response);

        Assert.Equal("handled:abc", response.Message);
        Assert.False(string.IsNullOrEmpty(response.ProcessedAt));
    }

    [Fact]
    public async Task Post_v2_test_returns_message_without_processed_at()
    {
        using var httpResponse = await ApiClient.PostAsJsonAsync(
            "/api/v2/test",
            new CreateTestRequest("abc"));

        httpResponse.EnsureSuccessStatusCode();
        var response = await httpResponse.Content.ReadFromJsonAsync<CreateTestResponseV2>();
        Assert.NotNull(response);

        Assert.Equal("handled:abc", response.Message);
    }

    [Fact]
    public async Task Post_test_with_missing_value_returns_400()
    {
        using var response = await ApiClient.PostAsJsonAsync("/api/v1/test", new { });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Healthcheck_returns_200()
    {
        using var response = await ApiClient.GetAsync("/healthcheck");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Request_without_tenant_header_returns_400()
    {
        using var client = BuildClientWithoutTenantHeaders();
        using var response = await client.GetAsync("/api/v1/test?value=hello");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Healthcheck_without_tenant_header_returns_200()
    {
        using var client = BuildClientWithoutTenantHeaders();
        using var response = await client.GetAsync("/healthcheck");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_v1_without_tenant_header_returns_200()
    {
        using var client = BuildClientWithoutTenantHeaders();
        using var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_v2_without_tenant_header_returns_200()
    {
        using var client = BuildClientWithoutTenantHeaders();
        using var response = await client.GetAsync("/swagger/v2/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
