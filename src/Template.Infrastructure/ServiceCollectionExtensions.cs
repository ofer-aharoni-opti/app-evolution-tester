using Microsoft.Extensions.DependencyInjection;
using Template.Application.Abstractions;
using Template.Infrastructure.Persistence;
using Template.Infrastructure.MultiTenancy;

namespace Template.Infrastructure;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure()
        {
            services
                .AddTenantApplicationContext()
                .AddPersistence()
                .AddServices();

            return services;
        }

        private IServiceCollection AddTenantApplicationContext()
        {
            services.AddScoped<ITenantApplicationContext, TenantApplicationContext>();
            services.AddTransient<TenantContextHandler>();

            return services;
        }

        private IServiceCollection AddPersistence()
        {
            services.AddSingleton<ITestRepository, TestRepository>();
            services.AddSingleton<IZubiRepository, ZubiRepository>();

            return services;
        }

        private void AddServices()
        {
            // Initialize services here
        }
    }
}
