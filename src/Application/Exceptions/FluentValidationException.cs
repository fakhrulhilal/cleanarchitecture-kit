using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using FM.Domain.Exception;

namespace FM.Application.Exceptions
{
    internal sealed class FluentValidationException : ValidationException
    {
        public FluentValidationException(IEnumerable<ValidationFailure> failures) : base(failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray()))
        {
        }
    }
}
