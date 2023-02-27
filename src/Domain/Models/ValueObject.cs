namespace DevKit.Domain.Models;

// Learn more: https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/microservice-ddd-cqrs-patterns/implement-value-objects
public abstract class ValueObject
{
    private static bool EqualOperator(ValueObject left, ValueObject right) => left.Equals(right);

    private static bool NotEqualOperator(ValueObject left, ValueObject right) => !EqualOperator(left, right);

    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj) {
        if (obj is null || obj.GetType() != GetType()) return false;

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public static bool operator ==(ValueObject a, ValueObject b) => EqualOperator(a, b);
    public static bool operator !=(ValueObject a, ValueObject b) => NotEqualOperator(a, b);

    public override int GetHashCode() => GetEqualityComponents()
        .Select(x => x.GetHashCode())
        .Aggregate((x, y) => x ^ y);
}
