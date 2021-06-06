using System.Threading;
using System.Threading.Tasks;
using FM.Domain.Event;

namespace FM.Application.Ports
{
    public interface IDomainEventDispatcher
    {
        Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken = new());
    }
}
