using System;
using Serilog.Configuration;

// ReSharper disable once CheckNamespace
namespace Serilog
{
    public static class SerilogExtensions
    {
        private const string PropertyName = "Environment";
        private const string DefaultValue = "Not Configured";

        /// <summary>
        /// Enrich log events with a EnvironmentName property containing the value of the ASPNETCORE_ENVIRONMENT or DOTNET_ENVIRONMENT environment variable.
        /// </summary>
        /// <param name="configuration">Logger enrichment configuration.</param>
        /// <param name="defaultValue">Default environment name when no value found (default: Not Configured)</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration WithEnvironmentName(this LoggerEnrichmentConfiguration configuration,
            string? defaultValue = null)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            string? environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(environmentName))
                environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(environmentName)) environmentName = defaultValue ?? DefaultValue;

            return configuration.WithProperty(PropertyName, environmentName);
        }
    }
}
