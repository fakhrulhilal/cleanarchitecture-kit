namespace FM.Domain.Exception
{
    /// <summary>
    /// Parent of all exception for the system
    /// </summary>
    public abstract class BusinessApplicationException : System.Exception
    {
        protected BusinessApplicationException()
        { }

        protected BusinessApplicationException(string message) : base(message)
        { }

        protected BusinessApplicationException(string message, System.Exception innerException)
            : base(message, innerException)
        { }
    }
}
