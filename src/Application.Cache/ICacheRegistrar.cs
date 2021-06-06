using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace FM.Application.Cache
{
    public interface ICacheRegistrar<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse?> GetAsync(TRequest request, CancellationToken cancellationToken);
        Task SetAsync(TRequest request, TResponse response, CancellationToken cancellationToken);
        Task RemoveAsync(string cacheKeyIdentifier, CancellationToken cancellationToken);
    }
}
