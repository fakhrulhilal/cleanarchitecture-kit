using DevKit.Domain.Exceptions;
using DevKit.Domain.Models;

namespace DevKit.Application.Behaviour;

/// <summary>
///     Log for unhandled exception for other than business error
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public sealed class UnhandledExceptionBehaviour<TRequest, TResponse>(
    ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly Type _businessException = typeof(BusinessApplicationException);

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken) {
        try {
            return await next();
        }
        catch (Exception exception) when (!_businessException.IsInstanceOfType(exception)) {
            string requestName = typeof(TRequest).GetClassName();
            logger.LogError(exception, "Unhandled exception for request {Name} {@Request}", requestName,
                request);
            throw;
        }
    }
}
