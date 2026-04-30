using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Template.WebApi.Middlewares;

public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    private static readonly string[] SkippedPathPrefixes = ["/swagger", "/healthcheck"];

    private const string MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {ElapsedMs:0.00} ms";

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkip(context))
        {
            await next(context);
            return;
        }

        var startTimestamp = Stopwatch.GetTimestamp();
        Exception? caughtException = null;

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            caughtException = ex;

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error"
                }, context.RequestAborted);
            }
        }
        finally
        {
            var elapsedMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
            var statusCode = context.Response.StatusCode;
            var args = new object[] { context.Request.Method, context.Request.Path.Value!, statusCode, elapsedMs };

            var level = caughtException is not null || statusCode >= 500
                ? LogLevel.Error
                : statusCode >= 400
                    ? LogLevel.Warning
                    : LogLevel.Information;

            logger.Log(level, caughtException, MessageTemplate, args);
        }
    }

    private static bool ShouldSkip(HttpContext context)
    {
        foreach (var prefix in SkippedPathPrefixes)
        {
            if (context.Request.Path.StartsWithSegments(prefix))
                return true;
        }

        return false;
    }
}
