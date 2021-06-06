using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FM.Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FM.Application.Behaviour
{
    public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly ILogger<ValidationBehaviour<TRequest, TResponse>> _logger;

        public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators,
            ILogger<ValidationBehaviour<TRequest, TResponse>> logger)
        {
            _logger = logger;
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            if (!_validators.Any()) return await next();

            var context = new ValidationContext<TRequest>(request);
            var validationResults =
                await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();
            if (failures.Count > 0)
            {
                _logger.LogDebug("Validation fails for {@Request} with these errors {@Errors}", request, failures);
                throw new FluentValidationException(failures);
            }

            return await next();
        }
    }
}
