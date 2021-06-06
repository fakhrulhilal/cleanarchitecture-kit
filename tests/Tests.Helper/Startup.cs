using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace FM.Tests
{
    public class Startup
    {
        private IServiceScope? _scope;
        private IServiceCollection? _services;

        private IServiceProvider Provider
        {
            get
            {
                if (_scope == null) throw new InvalidOperationException("Call Bootstrap before calling this");
                return _scope.ServiceProvider;
            }
        }

        public void CleanUp() => _scope?.Dispose();

        public TService GetService<TService>() where TService : notnull =>
            Provider.GetRequiredService<TService>();
        public TService GetService<TService>(Func<TService, bool> criteria) where TService : notnull =>
            Provider.GetServices<TService>().First(criteria);

        public void ConfigureServices(Action<IServiceCollection, IConfiguration>? configure = null, params Assembly[] assemblies)
        {
            // ensure we use English for validation message
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            _services ??= new ServiceCollection();
            AddJsonFileConfiguration(_services, out var config, "config.json")
                .AddCoreApplication(assemblies)
                .AddLogging(config, ConfigureLogging);
            configure?.Invoke(_services, config);
            _scope = _services.BuildServiceProvider().CreateScope();
        }

        public static void ConfigureLogging(LoggerConfiguration config) => config.WriteTo.NUnitOutput()
            .Destructure.ToMaximumDepth(3)
            .Destructure.ToMaximumStringLength(100)
            .Destructure.ToMaximumCollectionCount(10)
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning);

        private static IServiceCollection AddJsonFileConfiguration(IServiceCollection services,
            out IConfiguration config, string filename = "appsettings.json",
            Action<IConfigurationBuilder>? configure = null)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile(filename, true);
            configure?.Invoke(builder);
            var configuration = builder.Build();
            config = configuration;
            return services.AddSingleton<IConfiguration>(configuration);
        }
    }
}
