using Serilog;
using Serilog.Events;
using Template.WebApi;
using Template.WebApi.Middlewares;
using Template.WebApi.Swagger;
using Template.Application;
using Template.Infrastructure;
using Optimove.Infrastructure.Logger;


try
{
    var builder = WebApplication.CreateBuilder(args);

    ConfigureLogger(builder);
    
    ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

    var app = builder.Build();

    ConfigureApplication(app);

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static void ConfigureServices(IServiceCollection services, IConfigurationRoot configuration, IWebHostEnvironment environment)
{
    services.AddProblemDetails();
    services.AddHealthChecks();
    services.AddMemoryCache();
    services.AddApplication();
    services.AddApiControllers();
    services.AddApiVersioningConfiguration();
    services.AddCorsConfiguration(configuration, environment);
    services.AddMapster();
    services.AddInfrastructure();
    services.AddSwagger();
}

static void ConfigureLogger(WebApplicationBuilder builder)
{
    var configuration = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build();

    Action<LoggerConfiguration> loggerConfigOverrides = config =>
        config
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning);

    var logger = OptiLogger.Init(configuration, loggerConfigOverrides);

#if DEBUG
    var debugLoggerConfig = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console(
            outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
            theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
        .Enrich.FromLogContext();
    loggerConfigOverrides.Invoke(debugLoggerConfig);
    logger = debugLoggerConfig.CreateLogger();
#endif

    Log.Logger = logger;
    builder.Host.UseSerilog(Log.Logger);
}

static void ConfigureApplication(WebApplication app)
{
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseCors("DefaultCors");
    app.UseVersionedSwaggerUI();
    app.UseRouting();
    app.UseMiddleware<TenantContextMiddleware>();
    app.UseHttpsRedirection();
    app.MapControllers();
    app.MapHealthChecks("/healthcheck");
}
