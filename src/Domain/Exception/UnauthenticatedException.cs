namespace FM.Domain.Exception
{
    /// <summary>
    /// Unauthenticated user to access system
    /// </summary>
    public class UnauthenticatedException : BusinessApplicationException
    {
        public UnauthenticatedException()
        { }

        public UnauthenticatedException(string message) : base(message)
        { }
    }
}
