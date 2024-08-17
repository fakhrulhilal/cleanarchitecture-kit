using System.Diagnostics;
using DevKit.Application.Ports;
using DevKit.Domain.Models;
using Microsoft.Extensions.Options;

namespace DevKit.Application.Behaviour;

/// <summary>
///     Log request that takes longer than configured max long running task (default: 500ms)
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public sealed class PerformanceBehaviour<TRequest, TResponse>(
    ILogger<PerformanceBehaviour<TRequest, TResponse>> logger,
    ICurrentUserService currentUserService,
    IIdentityService identityService,
    IOptions<GeneralConfig> systemConfig)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly GeneralConfig _systemConfig = systemConfig.Value;
    private readonly Stopwatch _timer = new();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken) {
        _timer.Start();
        var response = await next();
        _timer.Stop();
        long elapsed = _timer.ElapsedMilliseconds;
        if (elapsed <= _systemConfig.MaxLongRunningTask) return response;

        string userId = currentUserService.UserId;
        string userName = string.Empty;
        if (!string.IsNullOrEmpty(userId)) userName = await identityService.GetUserNameAsync(userId);
        logger.LogWarning(
            "Long running request: {Name} ({Elapsed} milliseconds) {UserId} {UserName} {@Request}",
            typeof(TRequest).GetClassName(), elapsed, userId, userName, request);
        return response;
    }
}
