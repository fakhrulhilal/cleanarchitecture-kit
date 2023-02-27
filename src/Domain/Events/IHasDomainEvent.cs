namespace DevKit.Domain.Events;

public interface IHasDomainEvent
{
    List<IDomainEvent> DomainEvents { get; }
}
