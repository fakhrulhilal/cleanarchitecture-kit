namespace DevKit.Domain.Exceptions;

/// <summary>
///     Parent of all exception for the system
/// </summary>
public abstract class BusinessApplicationException : Exception
{
    protected BusinessApplicationException() { }

    protected BusinessApplicationException(string message) : base(message) { }

    protected BusinessApplicationException(string message, Exception innerInner)
        : base(message, innerInner) {
    }
}
