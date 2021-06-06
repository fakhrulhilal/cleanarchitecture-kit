using System.Threading;
using System.Threading.Tasks;

namespace FM.Application.Cache
{
    public interface ICacheRemover<in TRequest>
    {
        Task RemoveAsync(TRequest request, CancellationToken cancellationToken);
    }
}
