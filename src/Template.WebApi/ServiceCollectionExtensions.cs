using System.Text.Json;
using Asp.Versioning;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Serilog;
using Template.WebApi.Routing;
using Template.WebApi.Swagger;

namespace Template.WebApi;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApiControllers()
        {
            services.AddControllers(options =>
            {
                // transform controller route tokens to kebab-case
                options.Conventions.Add(
                    new RouteTokenTransformerConvention(
                        new KebabCaseParameterTransformer()));
            }).AddJsonOptions(options =>
            {
                //transform json to camelCase
                options.JsonSerializerOptions.PropertyNamingPolicy =
                    JsonNamingPolicy.CamelCase;
            });

            return services;
        }

        public IServiceCollection AddApiVersioningConfiguration()
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }

        public IServiceCollection AddMapster()
        {
            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(
                typeof(Application.AssemblyReference).Assembly,
                typeof(IApiMarker).Assembly);

            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();

            return services;
        }

        public IServiceCollection AddSwagger()
        {
            services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(t => t.FullName!.Replace("+", "."));
                c.OperationFilter<TenantHeadersOperationFilter>();
            });
            services.ConfigureOptions<ConfigureSwaggerVersions>();

            return services;
        }

        public IServiceCollection AddCorsConfiguration(IConfiguration configuration, IWebHostEnvironment environment)
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            var allowedOriginSuffixes = configuration.GetSection("Cors:AllowedOriginSuffixes").Get<string[]>() ?? Array.Empty<string>();
            var devAllowedOrigins = configuration.GetSection("Cors:DevAllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            var allowCors = Environment.GetEnvironmentVariable("ALLOW_CORS") == "true";
            Log.Information("CORS allow override from ALLOW_CORS is set to {AllowCors}", allowCors);
            Log.Information("Allowed dev origins: {DevAllowedOrigins}", string.Join(',', devAllowedOrigins));

            var allowLocalhostInDev = (allowCors || environment.IsDevelopment()) && devAllowedOrigins.Length > 0;

            services.AddCors(options =>
            {
                options.AddPolicy("DefaultCors", policy =>
                {
                    if (allowedOrigins.Length > 0 || allowedOriginSuffixes.Length > 0 || allowLocalhostInDev)
                    {
                        policy.SetIsOriginAllowed(origin =>
                        {
                            if (allowLocalhostInDev && devAllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                            {
                                return true;
                            }

                            if (allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                            {
                                return true;
                            }

                            return allowedOriginSuffixes.Any(suffix =>
                                origin.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
                        });
                    }

                    policy.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            return services;
        }
    }
}