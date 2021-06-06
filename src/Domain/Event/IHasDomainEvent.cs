using System.Collections.Generic;

namespace FM.Domain.Event
{
    public interface IHasDomainEvent
    {
        List<IDomainEvent> DomainEvents { get; }
    }
}
