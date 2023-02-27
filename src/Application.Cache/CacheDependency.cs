using System.Reflection;
using DevKit.Application.Behaviour;
using DevKit.Application.Ports;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class CacheDependency
{
    /// <summary>
    ///     Register cache registrar/invalidator behavior.
    ///     This will scan all classes implementing <see cref="ICacheRegistrar{TRequest,TResponse}" /> and
    ///     <see cref="ICacheRemover{TRequest}" />.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembliesToScan">
    ///     Add more assembly to scan the implementation. Executing assembly will always be added by default.
    /// </param>
    /// <returns></returns>
    public static IServiceCollection AddCacheKit(this IServiceCollection services,
        params Assembly[] assembliesToScan) {
        var targetAssemblies = new List<Assembly> {
            Assembly.GetExecutingAssembly(), typeof(CacheDependency).GetTypeInfo().Assembly
        };
        if (assembliesToScan.Length > 0) targetAssemblies.AddRange(assembliesToScan);
        var assemblies = targetAssemblies.Distinct().ToArray();

        // Configure MediatR Pipeline with cache invalidation and cached request behaviors
        services = services
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(CacheBehavior<,>));
        foreach (var (impl, contract) in assemblies.GetImplementations(typeof(ICacheRegistrar<,>),
                     typeof(ICacheRemover<>)))
            services = services.AddScoped(contract, impl);
        return services;
    }

    private static IEnumerable<ImplMap> GetImplementations(this Assembly[] assemblies,
        params Type[] contracts) {
        var implementations = assemblies.SelectMany(a => a.GetExportedTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .Where(type =>
                type.GetInterfaces().Any(i =>
                    i.IsGenericType && contracts.Contains(i.GetGenericTypeDefinition())))
            .Distinct();
        foreach (var impl in implementations)
        foreach (var contract in impl.GetInterfaces())
            yield return new(impl, contract);
    }

    private sealed record ImplMap(Type Implementation, Type Contract);
}
