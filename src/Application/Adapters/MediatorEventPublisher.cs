using System;
using System.Threading;
using System.Threading.Tasks;
using FM.Application.Ports;
using FM.Domain.Common;
using FM.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FM.Application.Adapters
{
    public class MediatorEventPublisher : IDomainEventDispatcher
    {
        private readonly IPublisher _publisher;
        private readonly ILogger<MediatorEventPublisher> _logger;

        public MediatorEventPublisher(IPublisher publisher, ILogger<MediatorEventPublisher> logger)
        {
            _logger = logger;
            _publisher = publisher;
        }

        public async Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken = new())
        {
            var domainEventType = domainEvent.GetType();
            _logger.LogDebug("Publishing domain event: {event}", domainEventType.GetClassName());
            await _publisher.Publish(ConvertToNotification(domainEventType, domainEvent), cancellationToken);
        }

        private static INotification ConvertToNotification(Type type, IDomainEvent domainEvent) =>
            (INotification?)Activator.CreateInstance(typeof(DomainEventNotification<>).MakeGenericType(type),
                domainEvent) ?? throw new InvalidOperationException("Failed to wrap domain notification");
    }
}
