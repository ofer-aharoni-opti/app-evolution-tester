using Microsoft.Extensions.DependencyInjection;

namespace Template.Application;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssemblyContaining<AssemblyReference>());

            return services;
        }
    }
}
