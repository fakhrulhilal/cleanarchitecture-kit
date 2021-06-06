using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FM.Application.Ports;
using FM.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FM.Application.Behaviour
{
    /// <summary>
    /// Log request that takes longer than configured max long running task (default: 500ms)
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly Stopwatch _timer;
        private readonly ILogger<TRequest> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IIdentityService _identityService;
        private readonly GeneralConfig _systemConfig;

        public PerformanceBehaviour(
            ILogger<TRequest> logger,
            ICurrentUserService currentUserService,
            IIdentityService identityService,
            GeneralConfig systemConfig)
        {
            _systemConfig = systemConfig;
            _timer = new Stopwatch();
            _logger = logger;
            _currentUserService = currentUserService;
            _identityService = identityService;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            _timer.Start();
            var response = await next();
            _timer.Stop();
            long elapsed = _timer.ElapsedMilliseconds;
            if (elapsed <= _systemConfig.MaxLongRunningTask) return response;

            string userId = _currentUserService.UserId;
            string userName = string.Empty;
            if (!string.IsNullOrEmpty(userId)) userName = await _identityService.GetUserNameAsync(userId);
            string message = "Long running request: {Name} ({Elapsed} milliseconds) {UserId} {UserName} {@Request}";
            _logger.LogWarning(message, typeof(TRequest).GetClassName(), elapsed, userId, userName, request);
            return response;
        }
    }
}
