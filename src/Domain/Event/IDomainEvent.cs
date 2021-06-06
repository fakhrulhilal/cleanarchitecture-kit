using System;

namespace FM.Domain.Event
{
    public interface IDomainEvent
    {
        bool IsPublished { get; set; }
        DateTimeOffset DateOccurred { get; }
    }
}
