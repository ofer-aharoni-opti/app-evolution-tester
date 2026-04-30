using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Template.WebApi.Middlewares;

namespace Template.WebApi.UnitTests.Middlewares;

public sealed class RequestLoggingMiddlewareTests
{
    [Fact]
    public async Task Successful_request_logs_at_information()
    {
        var (logger, middleware) = CreateMiddleware(_ => Task.CompletedTask);
        var context = CreateContext("/api/v1/test");

        await middleware.InvokeAsync(context);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, entry.Level);
    }

    [Fact]
    public async Task Client_error_logs_at_warning()
    {
        var (logger, middleware) = CreateMiddleware(ctx =>
        {
            ctx.Response.StatusCode = 400;
            return Task.CompletedTask;
        });
        var context = CreateContext("/api/v1/test");

        await middleware.InvokeAsync(context);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
    }

    [Fact]
    public async Task Server_error_logs_at_error()
    {
        var (logger, middleware) = CreateMiddleware(ctx =>
        {
            ctx.Response.StatusCode = 500;
            return Task.CompletedTask;
        });
        var context = CreateContext("/api/v1/test");

        await middleware.InvokeAsync(context);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
    }

    [Fact]
    public async Task Unhandled_exception_returns_problem_details_and_logs_error()
    {
        var thrownException = new InvalidOperationException("boom");
        var (logger, middleware) = CreateMiddleware(_ => throw thrownException);
        var context = CreateContext("/api/v1/test");
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        context.Response.Body.Position = 0;
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            context.Response.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Same(thrownException, entry.Exception);
    }

    [Theory]
    [InlineData("/swagger")]
    [InlineData("/swagger/v1/swagger.json")]
    [InlineData("/healthcheck")]
    public async Task Skipped_paths_are_not_logged(string path)
    {
        var nextCalled = false;
        var (logger, middleware) = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreateContext(path);

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Empty(logger.Entries);
    }

    [Theory]
    [InlineData("/api/v1/test")]
    [InlineData("/some/other/path")]
    public async Task Non_skipped_paths_are_logged(string path)
    {
        var (logger, middleware) = CreateMiddleware(_ => Task.CompletedTask);
        var context = CreateContext(path);

        await middleware.InvokeAsync(context);

        Assert.Single(logger.Entries);
    }

    private static DefaultHttpContext CreateContext(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = path;
        return context;
    }

    private static (FakeLogger<RequestLoggingMiddleware>, RequestLoggingMiddleware) CreateMiddleware(
        RequestDelegate next)
    {
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var middleware = new RequestLoggingMiddleware(next, logger);
        return (logger, middleware);
    }
}

internal sealed record LogEntry(LogLevel Level, Exception? Exception, string? Message);

internal sealed class FakeLogger<T> : ILogger<T>
{
    public List<LogEntry> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Entries.Add(new LogEntry(logLevel, exception, formatter(state, exception)));
    }
}
