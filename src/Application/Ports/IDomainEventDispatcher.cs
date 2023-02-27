using DevKit.Domain.Events;

namespace DevKit.Application.Ports;

public interface IDomainEventDispatcher
{
    Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken = new());
}
