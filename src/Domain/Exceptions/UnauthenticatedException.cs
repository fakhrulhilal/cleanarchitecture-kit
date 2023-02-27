namespace DevKit.Domain.Exceptions;

/// <summary>
///     Unauthenticated user to access system
/// </summary>
public class UnauthenticatedException : BusinessApplicationException
{
    public UnauthenticatedException() { }

    public UnauthenticatedException(string message) : base(message) { }
}
