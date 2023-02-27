using DevKit.Domain.Exceptions;
using FluentValidation.Results;

namespace DevKit.Application.Exceptions;

internal sealed class FluentValidationException : ValidationException
{
    public FluentValidationException(IEnumerable<ValidationFailure> failures) : base(failures
        .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
        .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray())) {
    }
}
