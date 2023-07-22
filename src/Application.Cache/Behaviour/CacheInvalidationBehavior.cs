using DevKit.Application.Ports;
using DevKit.Domain.Models;

namespace DevKit.Application.Behaviour;

/// <summary>
///     Courtesy of https://github.com/Imprise/Imprise.MediatR.Extensions.Caching
///     When injected into a MediatR pipeline, this behavior will receive a list of instances of
///     <see cref="ICacheRemover{TRequest}" />
///     instances for every class in your project that implements this interface for the given generic types.
///     When the request pipeline runs, the behavior will make sure the current request runs through the pipeline
///     and
///     after it returns will proceed to call Invalidate on any <see cref="ICacheRemover{TRequest}" /> instance
///     in the list of cache remover.
/// </summary>
/// <typeparam name="TRequest">The type of the request that needs to invalidate other cached request responses.</typeparam>
/// <typeparam name="TResponse">
///     The response of the request that causes invalidation of other cached request
///     responses.
/// </typeparam>
public sealed class CacheInvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly List<ICacheRemover<TRequest>> _cacheRemovers;
    private readonly ILogger<CacheInvalidationBehavior<TRequest, TResponse>> _logger;

    public CacheInvalidationBehavior(ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger,
        IEnumerable<ICacheRemover<TRequest>> cacheRemovers) {
        _logger = logger;
        _cacheRemovers = cacheRemovers.ToList();
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken) {
        // run through the request handler pipeline for this request
        var result = await next();
        string requestName = typeof(TRequest).GetClassName();

        // now loop through each cache remover for this request type and call the Invalidate method passing
        // an instance of this request in order to retrieve a cache key.
        foreach (var cache in _cacheRemovers) {
            _logger.LogDebug("Removing cache after getting {RequestName}", requestName);
            await cache.RemoveAsync(request, cancellationToken);
        }

        return result;
    }
}
