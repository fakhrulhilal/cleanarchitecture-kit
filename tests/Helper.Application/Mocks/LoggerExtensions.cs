using Microsoft.Extensions.Logging;

namespace DevKit.Application.Mocks;

public static class LoggerExtensions
{
    public static void Capture<T>(this Mock<ILogger<T>> logger, Action<string> callback,
        Action<Exception>? errorCallback = null, LogLevel? expectedLogLevel = null) => logger
        .Setup(x => x.Log(
            It.Is<LogLevel>(level => !expectedLogLevel.HasValue || expectedLogLevel == level),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
        .Callback(new InvocationAction(invocation =>
        {
            object state = invocation.Arguments[2];
            var exception = (Exception)invocation.Arguments[3];
            errorCallback?.Invoke(exception);
            object formatter = invocation.Arguments[4];
            var invokeMethod = formatter.GetType().GetMethod("Invoke");
            string logMessage = invokeMethod?.Invoke(formatter, new[] { state, exception })?.ToString() ??
                                string.Empty;
            callback(logMessage);
        }));
}
