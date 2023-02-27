using DevKit.Application.Adapters;
using DevKit.Domain.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DevKit;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediatRHandler<TRequest>(this IServiceCollection services,
        Func<TRequest, CancellationToken, Task>? callback = null)
        where TRequest : IRequest<Unit> =>
        services.AddTransient(_ =>
        {
            var handler = new Mock<IRequestHandler<TRequest, Unit>>();
            if (callback != null)
                handler.Setup(x => x.Handle(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
                    .Returns<TRequest, CancellationToken>(async (request, token) =>
                    {
                        await callback(request, token);
                        return Unit.Value;
                    });
            return handler.Object;
        });

    public static IServiceCollection AddMediatRHandler<TRequest, TResponse>(this IServiceCollection services,
        Func<TRequest, TResponse>? factory = null)
        where TRequest : IRequest<TResponse> =>
        services.AddTransient(_ =>
        {
            var handler = new Mock<IRequestHandler<TRequest, TResponse>>();
            if (factory != null)
                handler.Setup(x => x.Handle(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()).Result)
                    .Returns<TRequest, CancellationToken>((request, _) => factory(request));
            return handler.Object;
        });

    public static IServiceCollection AddMediatorEventHandler<TDomainEvent>(this IServiceCollection services,
        Action<TDomainEvent>? callback = null)
        where TDomainEvent : IDomainEvent =>
        services.AddTransient(_ =>
        {
            var handler = new Mock<INotificationHandler<DomainEventNotification<TDomainEvent>>>();
            if (callback != null)
                handler.Setup(x => x.Handle(It.IsAny<DomainEventNotification<TDomainEvent>>(),
                        It.IsAny<CancellationToken>()))
                    .Returns<DomainEventNotification<TDomainEvent>, CancellationToken>((wrappedEvent, _) =>
                    {
                        callback(wrappedEvent.DomainEvent);
                        return Task.CompletedTask;
                    });
            return handler.Object;
        });

    public static IServiceCollection AddFakeLogger<TContext>(this IServiceCollection services,
        Action<string> callback, Action<Exception>? errorCallback = null,
        LogLevel expectedLogLevel = LogLevel.Debug) => services
        .AddTransient(_ =>
        {
            var logger = new Mock<ILogger<TContext>>();
            logger.CaptureLog(callback, errorCallback, expectedLogLevel);
            return logger.Object;
        });
}
