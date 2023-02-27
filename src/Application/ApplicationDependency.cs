using System.Reflection;
using System.Text.RegularExpressions;
using DevKit.Application.Adapters;
using DevKit.Application.Behaviour;
using DevKit.Application.Ports;
using DevKit.Domain.Models;
using FluentValidation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ApplicationDependency
{
    private static readonly Regex _configName =
        new(@"(Config)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static IServiceCollection AddApplicationKit(this IServiceCollection services,
        params Assembly[] assembliesToScan) {
        var targetAssemblies = new List<Assembly> {
            Assembly.GetExecutingAssembly(), typeof(ApplicationDependency).GetTypeInfo().Assembly
        };
        if (assembliesToScan.Length > 0) targetAssemblies.AddRange(assembliesToScan);
        var assemblies = targetAssemblies.Distinct().ToArray();
        return services
            .AddConfig<GeneralConfig>()
            .AddMediator(assemblies)
            .AddValidatorsFromAssemblies(assemblies)
            .AddScoped<IDomainEventDispatcher, MediatorEventPublisher>();
    }

    public static IServiceCollection AddConfig<TConfig>(this IServiceCollection services,
        string? configName = null) where TConfig : class {
        configName ??= _configName.Replace(typeof(TConfig).Name, string.Empty);
        services.AddOptions<TConfig>().BindConfiguration(configName);
        return services;
    }

    private static IServiceCollection AddMediator(this IServiceCollection services, Assembly[] assemblies) =>
        services
            .AddMediatR(assemblies, config => config.AsScoped())
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
}
