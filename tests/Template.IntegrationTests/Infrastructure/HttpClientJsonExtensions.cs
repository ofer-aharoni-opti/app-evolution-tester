using System.Net.Http.Json;
using System.Text.Json;

namespace Template.IntegrationTests.Infrastructure;

public static class HttpClientJsonExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        // Future-proofing for kebab-case responses containing hyphens.
        PropertyNameCaseInsensitive = true,
    };

    public static async Task<T> GetJsonAsync<T>(this HttpClient client, string requestUri, CancellationToken ct = default)
    {
        using var response = await client.GetAsync(requestUri, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
        return result ?? throw new InvalidOperationException("Response body was empty.");
    }

    public static async Task<TResponse> PostJsonAsync<TRequest, TResponse>(
        this HttpClient client,
        string requestUri,
        TRequest request,
        CancellationToken ct = default)
    {
        using var response = await client.PostAsJsonAsync(requestUri, request, JsonOptions, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct);
        return result ?? throw new InvalidOperationException("Response body was empty.");
    }
}


