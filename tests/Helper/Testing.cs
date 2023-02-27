using System.Globalization;
using System.Reflection;
using Bogus;
using Microsoft.Extensions.Configuration;

namespace DevKit;

public delegate void SetupService(IServiceCollection services);

public static class Testing
{
    public static readonly Faker Faker = new();
    public static Dictionary<string, string?> Configurations { get; } = new();

    public static IServiceProvider ConfigureServices(SetupService setup) =>
        ConfigureServices((services, _) => setup(services));
    public static IServiceProvider ConfigureServices(Action<IServiceCollection, IConfiguration> setup) {
        // ensure we use English for validation message
        CultureInfo.CurrentUICulture = new("en-US");
        var config = BuildConfiguration();
        var services = new ServiceCollection();
        services.AddDefault(config);
        setup(services, config);
        return services.BuildServiceProvider(false);
    }

    private static void AddDefault(this IServiceCollection services, IConfiguration config) => services
        .AddLogging(config, log => log.ConfigureTestLog());

    private static IConfiguration BuildConfiguration() => new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", true)
        .AddUserSecrets(Assembly.GetCallingAssembly(), true)
        .AddInMemoryCollection(Configurations)
        .Build();
}
