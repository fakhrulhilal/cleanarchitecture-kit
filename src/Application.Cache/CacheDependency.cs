using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FM.Application.Cache;
using FM.Application.Cache.Behaviour;
using MediatR;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class CacheDependency
    {
        public static IServiceCollection AddCache(this IServiceCollection services, params Assembly[] assembliesToScan)
        {
            var targetAssemblies = new List<Assembly>
                { Assembly.GetExecutingAssembly(), typeof(CacheDependency).GetTypeInfo().Assembly };
            if (assembliesToScan.Length > 0) targetAssemblies.AddRange(assembliesToScan);
            var assemblies = targetAssemblies.Distinct().ToArray();

            // Configure MediatR Pipeline with cache invalidation and cached request behaviors
            services = services
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(CacheBehavior<,>));
            foreach (var (impl, contract) in assemblies.GetImplementations(typeof(ICacheRegistrar<,>), typeof(ICacheRemover<>)))
                services = services.AddScoped(contract, impl);
            return services;
        }

        private static IEnumerable<(Type Implementation, Type Contract)> GetImplementations(this Assembly[] assemblies,
            params Type[] contracts)
        {
            var implementations = assemblies.SelectMany(a => a.GetExportedTypes())
                .Where(type => type.IsClass && !type.IsAbstract)
                .Where(type =>
                    type.GetInterfaces().Any(i => i.IsGenericType && contracts.Contains(i.GetGenericTypeDefinition())))
                .Distinct();
            foreach (var impl in implementations)
                foreach (var contract in impl.GetInterfaces())
                    yield return (impl, contract);
        }
    }
}
