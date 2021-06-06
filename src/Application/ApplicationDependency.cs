using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentValidation;
using FM.Application.Adapters;
using FM.Application.Behaviour;
using FM.Application.Ports;
using FM.Domain.Common;
using MediatR;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApplicationDependency
    {
        private static readonly Regex _configName = new(@"(Config)$", RegexOptions.IgnoreCase);

        public static IServiceCollection AddCoreApplication(this IServiceCollection services,
            params Assembly[] assembliesToScan)
        {
            var targetAssemblies = new List<Assembly>
                { Assembly.GetExecutingAssembly(), typeof(ApplicationDependency).GetTypeInfo().Assembly };
            if (assembliesToScan.Length > 0) targetAssemblies.AddRange(assembliesToScan);
            var assemblies = targetAssemblies.Distinct().ToArray();
            return services
                .AddSingleton(provider => provider.GetConfig<GeneralConfig>())
                .AddMediator(assemblies)
                .AddValidatorsFromAssemblies(assemblies)
                .AddScoped<IDomainEventDispatcher, MediatorEventPublisher>();
        }

        public static TConfig GetConfig<TConfig>(this IServiceProvider provider, string key) =>
            provider.GetRequiredService<IConfiguration>().GetConfig<TConfig>(key);

        public static TConfig GetConfig<TConfig>(this IServiceProvider provider) =>
            provider.GetConfig<TConfig>(_configName.Replace(typeof(TConfig).Name, string.Empty));

        public static TConfig GetConfig<TConfig>(this IConfiguration config) =>
            config.GetConfig<TConfig>(_configName.Replace(typeof(TConfig).Name, string.Empty));

        public static TConfig GetConfig<TConfig>(this IConfiguration config, string key) =>
            config.GetSection(key).Get<TConfig>();

        private static IServiceCollection AddMediator(this IServiceCollection services, Assembly[] assemblies) =>
            services
                .AddMediatR(assemblies, config => config.AsScoped())
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
    }
}
