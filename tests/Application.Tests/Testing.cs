using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace DevKit.Application.Tests;

[SetUpFixture]
internal class Testing
{
    private static readonly Startup _bootstrapper = new();
    private static readonly object _lock = new();
    private static int _testId;

    private static readonly Assembly[] _defaultAssemblies = { typeof(Testing).GetTypeInfo().Assembly };

    internal static int NextSeed
    {
        get
        {
            lock (_lock) return ++_testId;
        }
    }

    internal static TService GetService<TService>() where TService : notnull =>
        _bootstrapper.GetService<TService>();

    [OneTimeTearDown]
    public void TearDown() => _bootstrapper.CleanUp();

    [OneTimeSetUp]
    public void SetUp() {
        _testId = TestContext.CurrentContext.Random.NextShort(11, 100);
        _bootstrapper.ConfigureServices(WithDefaultSetup, _defaultAssemblies);
    }

    private static void WithDefaultSetup(IServiceCollection services, IConfiguration config) => services
        .AddTransient(_ => Mock.Of<ICurrentUserService>())
        .AddTransient(_ => Mock.Of<IIdentityService>());

    public static Startup ConfigureServices(Action<IServiceCollection> configure) {
        var bootstrapper = new Startup();
        bootstrapper.ConfigureServices((services, config) =>
        {
            WithDefaultSetup(services, config);
            configure(services);
        }, _defaultAssemblies);
        return bootstrapper;
    }
}
