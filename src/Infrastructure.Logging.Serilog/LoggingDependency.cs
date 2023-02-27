using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Exceptions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class LoggingDependency
{
    public static IServiceCollection AddLogging(this IServiceCollection services, IConfiguration config,
        Action<LoggerConfiguration>? configure = null) => services
        .AddLogging(builder =>
        {
            var logConfig = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithExceptionDetails()
                .ReadFrom.Configuration(config)
                .Destructure.ByCustomRules(rules => rules
                    .ByProperRecordProcessing()
                    .IgnoreStatic()
                    .IgnoreGeneratedByCompiler()
                    .Masking("***", new[] { "Password" }));
            configure?.Invoke(logConfig);
            var logger = logConfig.CreateLogger();
            _ = builder.ClearProviders().AddSerilog(logger);
        });
}
