namespace FM.Domain.Exception
{
    public class NotFoundException : BusinessApplicationException
    {
        public NotFoundException() : base("Item not found.")
        { }

        protected NotFoundException(string message) : base(message)
        { }

        public string Entity { get; protected init; } = string.Empty;
    }
}
