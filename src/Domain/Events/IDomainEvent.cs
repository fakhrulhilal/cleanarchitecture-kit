namespace DevKit.Domain.Events;

public interface IDomainEvent
{
    bool IsPublished { get; set; }
    DateTimeOffset DateOccurred { get; }
}
