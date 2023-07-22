using System.Reflection;

namespace DevKit.Application.Tests;

internal class Testing
{
    private static readonly Assembly[] _defaultAssemblies = { typeof(Testing).GetTypeInfo().Assembly };

    public static IServiceProvider Configure(SetupService setup) => DevKit.Testing.Configure(services =>
    {
        services.Mock<ICurrentUserService>();
        services.Mock<IIdentityService>();
        services.MockConfig<GeneralConfig>();
        services.AddApplicationKit(_defaultAssemblies);
        setup(services);
    });
}
