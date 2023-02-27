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
public sealed class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;
    private readonly ILogger<PerformanceBehaviour<TRequest, TResponse>> _logger;
    private readonly GeneralConfig _systemConfig;
    private readonly Stopwatch _timer = new();

    public PerformanceBehaviour(
        ILogger<PerformanceBehaviour<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService,
        IIdentityService identityService,
        IOptions<GeneralConfig> systemConfig) {
        _systemConfig = systemConfig.Value;
        _logger = logger;
        _currentUserService = currentUserService;
        _identityService = identityService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken) {
        _timer.Start();
        var response = await next();
        _timer.Stop();
        long elapsed = _timer.ElapsedMilliseconds;
        if (elapsed <= _systemConfig.MaxLongRunningTask) return response;

        string userId = _currentUserService.UserId;
        string userName = string.Empty;
        if (!string.IsNullOrEmpty(userId)) userName = await _identityService.GetUserNameAsync(userId);
        _logger.LogWarning(
            "Long running request: {Name} ({Elapsed} milliseconds) {UserId} {UserName} {@Request}",
            typeof(TRequest).GetClassName(), elapsed, userId, userName, request);
        return response;
    }
}
