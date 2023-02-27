using DevKit.Application.Ports;
using DevKit.Domain.Events;
using DevKit.Domain.Models;

namespace DevKit.Application.Adapters;

public class MediatorEventPublisher : IDomainEventDispatcher
{
    private readonly ILogger<MediatorEventPublisher> _logger;
    private readonly IPublisher _publisher;

    public MediatorEventPublisher(IPublisher publisher, ILogger<MediatorEventPublisher> logger) {
        _logger = logger;
        _publisher = publisher;
    }

    public async Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken = new()) {
        var domainEventType = domainEvent.GetType();
        _logger.LogDebug("Publishing domain event: {event}", domainEventType.GetClassName());
        await _publisher.Publish(ConvertToNotification(domainEventType, domainEvent), cancellationToken);
    }

    private static INotification ConvertToNotification(Type type, IDomainEvent domainEvent) =>
        (INotification?)Activator.CreateInstance(typeof(DomainEventNotification<>).MakeGenericType(type),
            domainEvent) ?? throw new InvalidOperationException("Failed to wrap domain notification");
}
