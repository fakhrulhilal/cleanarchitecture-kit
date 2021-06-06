using System.Threading;
using System.Threading.Tasks;
using FM.Domain.Common;
using FM.Domain.Exception;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FM.Application.Behaviour
{
    /// <summary>
    /// Log for unhandled exception for other than business error
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<TRequest> _logger;
        private readonly System.Type _businessException = typeof(BusinessApplicationException);

        public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                return await next();
            }
            catch (System.Exception exception) when (!_businessException.IsInstanceOfType(exception))
            {
                string requestName = typeof(TRequest).GetClassName();
                _logger.LogError(exception, "Unhandled exception for request {Name} {@Request}", requestName, request);
                throw;
            }
        }
    }
}
