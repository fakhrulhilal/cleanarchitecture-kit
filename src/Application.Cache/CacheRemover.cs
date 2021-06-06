using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace FM.Application.Cache
{
    /// <summary>
    /// Courtesy of https://github.com/Imprise/Imprise.MediatR.Extensions.Caching
    /// Inherit from this class when you need to handle the case of one request type (TReques) invalidating the cached
    /// response (TCachedResponse) of a difference cached request (TCachedRequest)
    ///
    /// <example>
    /// e.g. GetUser : <c>IRequest&lt;User&gt;</c>  has cached the response to User. Another request UpdateUser : IRequest should
    /// invalidate this request. We can create a new 
    /// <code>class UpdateUserCacheRemover : CacheRemover&lt;UpdateUser, GetUser, User&gt;</code>
    /// that will cause the GetUser for a given user to be invalidated
    /// </example> 
    /// </summary>
    /// <typeparam name="TRequest">The type of the request that will run and cause a different cached request to be invalidated.</typeparam>
    /// <typeparam name="TCachedRequest">The type of the request that has been cached and should be invalidated by TRequest</typeparam>
    /// <typeparam name="TCachedResponse">The type of the response that is cached by a TCachedRequest.</typeparam>
    public abstract class CacheRemover<TRequest, TCachedRequest, TCachedResponse> : ICacheRemover<TRequest>
        where TCachedRequest : IRequest<TCachedResponse>
    {
        private readonly ICacheRegistrar<TCachedRequest, TCachedResponse> _cacheRegistrar;

        protected CacheRemover(ICacheRegistrar<TCachedRequest, TCachedResponse> cacheRegistrar)
        {
            _cacheRegistrar = cacheRegistrar;
        }

        protected abstract string GetRetrievingIdentifier(TRequest command);

        /// <summary>
        /// Removes the cached response entry for a request identified by this requests CacheKeyIdentifier
        /// </summary>
        /// <param name="request">The type of the request that will cause another cached request to be invalidated</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task RemoveAsync(TRequest request, CancellationToken cancellationToken) =>
            await _cacheRegistrar.RemoveAsync(GetRetrievingIdentifier(request), cancellationToken);
    }
}
