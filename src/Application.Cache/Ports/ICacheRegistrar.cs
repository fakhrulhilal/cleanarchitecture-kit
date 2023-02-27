namespace DevKit.Application.Ports;

/// <summary>
///     Define cache registration for specific <typeparamref name="TResponse" /> which is produced by request
///     handler of <typeparamref name="TRequest" />.
/// </summary>
/// <typeparam name="TRequest">Request</typeparam>
/// <typeparam name="TResponse">Response object that will be cached</typeparam>
public interface ICacheRegistrar<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    /// <summary>
    ///     Underlying implementation to get <typeparamref name="TResponse" />
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TResponse?> GetAsync(TRequest request, CancellationToken cancellationToken);

    /// <summary>
    ///     Store <typeparamref name="TResponse" /> to cache provider.
    /// </summary>
    /// <param name="request">
    ///     Original request producing the <paramref name="response" />.
    ///     The request will be used for generating unique identifier for the cache key.
    /// </param>
    /// <param name="response">Response object that will be cached</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetAsync(TRequest request, TResponse response, CancellationToken cancellationToken);

    Task RemoveAsync(string cacheKeyIdentifier, CancellationToken cancellationToken);
}
