using DevKit.Domain.Events;

namespace DevKit.Application.Adapters;

public class DomainEventNotification<TDomainEvent> : INotification where TDomainEvent : IDomainEvent
{
    public DomainEventNotification(TDomainEvent domainEvent) => DomainEvent = domainEvent;

    public TDomainEvent DomainEvent { get; }
}
