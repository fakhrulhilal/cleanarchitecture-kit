using Microsoft.Extensions.Caching.Distributed;

namespace DevKit.Application.Ports;

/// <summary>
///     Courtesy of https://github.com/Imprise/Imprise.MediatR.Extensions.Caching
///     Implement this class when you want your cached request response to be stored in a distributed cached
/// </summary>
/// <typeparam name="TRequest">The type of the request who's response will be cached.</typeparam>
/// <typeparam name="TResponse">The type of the response of the request that will be cached.</typeparam>
public abstract class CacheRegistrar<TRequest, TResponse>(IDistributedCache distributedCache)
    : ICacheRegistrar<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    protected virtual DateTime? AbsoluteExpiration { get; } = null;
    protected virtual TimeSpan? AbsoluteExpirationRelativeToNow { get; } = null;
    protected virtual TimeSpan? SlidingExpiration { get; } = null;

    public virtual async Task<TResponse?> GetAsync(TRequest request, CancellationToken cancellationToken) =>
        await distributedCache.GetAsync<TResponse>(GetCacheKey(GetRetrievingIdentifier(request)),
            cancellationToken);

    /// <summary>
    ///     Store response in the cache.
    ///     It will store the same cache item for different keys based on
    ///     <see cref="GetStoringIdentifiers(TRequest, TResponse)" />.
    /// </summary>
    /// <param name="request">Request initiator</param>
    /// <param name="response">Response to be cached</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task SetAsync(TRequest request, TResponse response,
        CancellationToken cancellationToken) =>
        await distributedCache.SetAsync(response,
            GetStoringIdentifiers(request, response).Select(GetCacheKey).ToArray(),
            new() {
                AbsoluteExpiration = AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = AbsoluteExpirationRelativeToNow,
                SlidingExpiration = SlidingExpiration
            }, cancellationToken);

    /// <summary>
    ///     Removes the response from cache using the <paramref name="cacheKeyIdentifier" /> from the Request.
    ///     It will also clean up same cache items that are defined based on
    ///     <see cref="GetStoringIdentifiers(TRequest, TResponse)" />.
    /// </summary>
    /// <param name="cacheKeyIdentifier">
    ///     A string identifier that uniquely identifies the response to be removed.
    ///     It must be initiated from <see cref="GetRetrievingIdentifier(TRequest)" />.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task RemoveAsync(string cacheKeyIdentifier, CancellationToken cancellationToken) =>
        distributedCache.RemoveAsync(GetCacheKey(cacheKeyIdentifier), cancellationToken);

    /// <summary>
    ///     Override and return a string key to uniquely identify the cached response when retrieving.
    ///     The result must be one of value generated by <see cref="GetStoringIdentifiers(TRequest, TResponse)" />.
    /// </summary>
    /// <param name="query">The type of the request who's response will be cached.</param>
    /// <returns></returns>
    protected abstract string GetRetrievingIdentifier(TRequest query);

    /// <summary>
    ///     Override and return a string key to uniquely identify when caching response
    /// </summary>
    /// <param name="request">Request initiator</param>
    /// <param name="response">Response result</param>
    /// <returns></returns>
    protected abstract IEnumerable<string> GetStoringIdentifiers(TRequest request, TResponse response);

    private static string GetCacheKey(string id) => $"{typeof(TRequest).FullName}:{id}";
}
