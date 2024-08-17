using DevKit.Domain.Exceptions;
using FluentValidation.Results;

namespace DevKit.Application.Exceptions;

internal sealed class FluentValidationException(IEnumerable<ValidationFailure> failures)
    : ValidationException(failures
        .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
        .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray()));
