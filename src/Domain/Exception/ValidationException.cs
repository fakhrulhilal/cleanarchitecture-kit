using System.Collections.Generic;

namespace FM.Domain.Exception
{
    /// <summary>
    /// Indicate that there's invalid data passed to the application before processing next step
    /// </summary>
    public class ValidationException : BusinessApplicationException
    {
        public ValidationException()
            : base("One or more validation failures have occurred.")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(IDictionary<string, string[]> errors)
            : this()
        {
            Errors = errors;
        }

        /// <summary>
        /// All errors where the key is the related data/property of class
        /// </summary>
        public IDictionary<string, string[]> Errors { get; }
    }
}
