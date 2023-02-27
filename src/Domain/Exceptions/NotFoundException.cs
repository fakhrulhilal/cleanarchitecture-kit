namespace DevKit.Domain.Exceptions;

public class NotFoundException : BusinessApplicationException
{
    public NotFoundException() : base("Item not found.") { }

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string message, Exception inner) : base(message, inner) {
    }

    public string Entity { get; protected init; } = string.Empty;
}
