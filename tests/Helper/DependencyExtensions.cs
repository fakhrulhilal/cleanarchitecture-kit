using Serilog;
using Serilog.Events;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyExtensions
{
    public static IServiceCollection Replace<TService>(this IServiceCollection services,
        Func<IServiceProvider, TService> implementationFactory,
        ServiceLifetime lifeTime = ServiceLifetime.Singleton) where TService : class {
        var serviceType = typeof(TService);
        var existing = services.FirstOrDefault(svc => svc.ServiceType == serviceType);
        if (existing != null) {
            services.Remove(existing);
        }

        var serviceDescriptor = new ServiceDescriptor(serviceType, implementationFactory, lifeTime);
        services.Add(serviceDescriptor);
        return services;
    }
    
    public static LoggerConfiguration ConfigureTestLog(this LoggerConfiguration config) => config
        .WriteTo.NUnitOutput()
        .Destructure.ToMaximumDepth(3)
        .Destructure.ToMaximumStringLength(100)
        .Destructure.ToMaximumCollectionCount(10)
        .MinimumLevel.Verbose()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .MinimumLevel.Override("System", LogEventLevel.Warning);
}
