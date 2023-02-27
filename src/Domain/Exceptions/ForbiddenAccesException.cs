namespace DevKit.Domain.Exceptions;

/// <summary>
///     Authenticated user but not no granted permission to access certain resource
/// </summary>
public class ForbiddenAccessException : BusinessApplicationException
{
    public ForbiddenAccessException() { }

    public ForbiddenAccessException(string message) : base(message) { }

    public ForbiddenAccessException(string message, Exception inner) : base(message, inner) { }
}
