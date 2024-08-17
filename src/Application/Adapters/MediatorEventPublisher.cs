using DevKit.Application.Ports;
using DevKit.Domain.Events;
using DevKit.Domain.Models;

namespace DevKit.Application.Adapters;

public class MediatorEventPublisher(IPublisher publisher, ILogger<MediatorEventPublisher> logger)
    : IDomainEventDispatcher
{
    public async Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken = new()) {
        var domainEventType = domainEvent.GetType();
        logger.LogDebug("Publishing domain event: {event}", domainEventType.GetClassName());
        await publisher.Publish(ConvertToNotification(domainEventType, domainEvent), cancellationToken);
    }

    private static INotification ConvertToNotification(Type type, IDomainEvent domainEvent) =>
        (INotification?)Activator.CreateInstance(typeof(DomainEventNotification<>).MakeGenericType(type),
            domainEvent) ?? throw new InvalidOperationException("Failed to wrap domain notification");
}
