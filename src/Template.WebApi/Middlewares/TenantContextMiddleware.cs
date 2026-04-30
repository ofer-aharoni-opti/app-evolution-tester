using System.Net;
using Microsoft.AspNetCore.Mvc;
using Template.Application.Abstractions;
using Template.WebApi.Attributes;
using Template.Application.Constants;

namespace Template.WebApi.Middlewares;

public sealed class TenantContextMiddleware(RequestDelegate next)
{
    private static readonly string[] SkippedPathPrefixes = ["/swagger", "/healthcheck"];

    public async Task InvokeAsync(HttpContext httpContext, ITenantApplicationContext tenantContext)
    {
        if (ShouldSkip(httpContext))
        {
            await next(httpContext);
            return;
        }

        var endpoint = httpContext.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<AllowWithNoTenantHeader>() is not null)
        {
            await next(httpContext);
            return;
        }

        if (!httpContext.Request.Headers.TryGetValue(TenantHeaders.TenantId, out var headerValue)
            || !int.TryParse(headerValue, out var tenantId)
            || tenantId <= 0)
        {
            await WriteProblemResponseAsync(httpContext, HttpStatusCode.BadRequest,
                $"A valid '{TenantHeaders.TenantId}' header is required.");
            return;
        }

        string? userName = httpContext.Request.Headers.TryGetValue(TenantHeaders.UserName, out var userNameValue)
            ? userNameValue.ToString()
            : null;

        var requiresUserName = endpoint?.Metadata.GetMetadata<RequireUserNameHeader>() is not null;
        if (requiresUserName && string.IsNullOrWhiteSpace(userName))
        {
            await WriteProblemResponseAsync(httpContext, HttpStatusCode.BadRequest,
                $"The '{TenantHeaders.UserName}' header is required for this endpoint.");
            return;
        }

        tenantContext.Initialize(tenantId, userName);

        using (Serilog.Context.LogContext.PushProperty("TenantId", tenantId))
        using (string.IsNullOrWhiteSpace(userName) ? null : Serilog.Context.LogContext.PushProperty("UserName", userName))
        {
            await next(httpContext);
        }
    }

    private static bool ShouldSkip(HttpContext httpContext)
    {
        foreach (var prefix in SkippedPathPrefixes)
        {
            if (httpContext.Request.Path.StartsWithSegments(prefix))
                return true;
        }

        return false;
    }

    private static async Task WriteProblemResponseAsync(HttpContext httpContext, HttpStatusCode statusCode, string detail)
    {
        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = statusCode.ToString(),
            Detail = detail
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, httpContext.RequestAborted);
    }
}
