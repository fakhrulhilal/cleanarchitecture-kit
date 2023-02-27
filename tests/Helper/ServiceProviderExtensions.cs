namespace DevKit;

public static class ServiceProviderExtensions
{
    /// <summary>
    /// An alias of <see cref="ServiceProviderServiceExtensions.GetRequiredService{TService}(IServiceProvider)"/>
    /// with shorter name.
    /// </summary>
    /// <param name="provider"></param>
    /// <typeparam name="TService"></typeparam>
    /// <returns>Resolved dependency or throw when not found</returns>
    public static TService Resolve<TService>(this IServiceProvider provider) where TService : notnull =>
        provider.GetRequiredService<TService>();

    public static TService Resolve<TService>(this IServiceProvider provider, Func<TService, bool> criteria)
        where TService : notnull =>
        provider.GetServices<TService>().First(criteria);
}
