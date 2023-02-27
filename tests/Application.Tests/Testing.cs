using System.Reflection;

namespace DevKit.Application.Tests;

internal class Testing
{
    private static readonly Assembly[] _defaultAssemblies = { typeof(Testing).GetTypeInfo().Assembly };

    public static IServiceProvider ConfigureServices(SetupService setup) =>
        DevKit.Testing.ConfigureServices(services => {
            services.Mock<ICurrentUserService>();
            services.Mock<IIdentityService>();
            services.MockConfig<GeneralConfig>();
            services.AddApplicationKit(_defaultAssemblies);
            setup(services);
        });
}
