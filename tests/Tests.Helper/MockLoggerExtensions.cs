using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace FM.Tests
{
    public static class MockLoggerExtensions
    {
        public static void CaptureLog<T>(this Mock<ILogger<T>> logger,
            Action<string> callback, Action<Exception>? errorCallback = null,
            LogLevel expectedLogLevel = LogLevel.Debug) => logger
            .Setup(x => x.Log(
                It.Is<LogLevel>(l => l == expectedLogLevel),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
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
}
