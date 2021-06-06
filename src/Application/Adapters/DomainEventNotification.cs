using FM.Domain.Event;
using MediatR;

namespace FM.Application.Adapters
{
    public class DomainEventNotification<TDomainEvent> : INotification where TDomainEvent : IDomainEvent
    {
        public DomainEventNotification(TDomainEvent domainEvent)
        {
            DomainEvent = domainEvent;
        }

        public TDomainEvent DomainEvent { get; }
    }
}
