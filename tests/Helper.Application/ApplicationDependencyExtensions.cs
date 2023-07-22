using DevKit.Application;
using DevKit.Application.Adapters;
using DevKit.Application.Mocks;
using DevKit.Domain.Events;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using static DevKit.Testing;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ApplicationDependencyExtensions
{
    public static IServiceCollection AddMock<T>(this IServiceCollection services, Mock<T> mock)
        where T : class => services.AddTransient(_ => mock.Object);
    public static IServiceCollection Mock<T>(this IServiceCollection services)
        where T : class => services.Mock<T>(_ => { });
    public static IServiceCollection Mock<T>(this IServiceCollection services, Action<Mock<T>> setup)
        where T : class => services.AddTransient(_ =>
    {
        var mock = new Mock<T>();
        setup.Invoke(mock);
        return mock.Object;
    });

    public static IServiceCollection MockHandler<TRequest>(this IServiceCollection services)
        where TRequest : IRequest<Unit> => services.MockHandler<TRequest, Unit>();
    
    public static IServiceCollection MockHandler<TRequest, TResponse>(this IServiceCollection services)
        where TRequest : IRequest<TResponse> => services.Mock<IRequestHandler<TRequest, TResponse>>(_ => { });

    public static IServiceCollection MockHandler<TRequest>(this IServiceCollection services,
        SyncCommandCallback<TRequest> callback) where TRequest : IRequest<Unit> =>
        services.MockHandler<TRequest, Unit>(request =>
        {
            callback(request);
            return Unit.Value;
        });
        
    public static IServiceCollection MockHandler<TRequest>(this IServiceCollection services,
        CommandCallback<TRequest> callback) where TRequest : IRequest<Unit> => services
        .Mock<IRequestHandler<TRequest, Unit>>(handler => handler
            .Setup(x => x.Handle(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
            .Returns(async (TRequest request, CancellationToken cancellationToken) =>
            {
                await callback(request, cancellationToken);
                return Unit.Value;
            }));

    public static IServiceCollection MockHandler<TRequest, TResponse>(this IServiceCollection services,
        SyncRequestCallback<TRequest, TResponse> callback) where TRequest : IRequest<TResponse> => services
        .MockHandler<TRequest, TResponse>((request, _) => Task.FromResult(callback(request)));

    public static IServiceCollection MockHandler<TRequest, TResponse>(this IServiceCollection services,
        RequestCallback<TRequest, TResponse> callback) where TRequest : IRequest<TResponse> => services
        .Mock<IRequestHandler<TRequest, TResponse>>(handler => handler
            .Setup(x => x.Handle(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
            .Returns(async (TRequest request, CancellationToken token) => await callback(request, token)));

    public static IServiceCollection MockEventHandler<TDomainEvent>(this IServiceCollection services,
        Action<TDomainEvent> callback) where TDomainEvent : IDomainEvent => services
        .Mock<INotificationHandler<DomainEventNotification<TDomainEvent>>>(handler => handler
            .Setup(x => x.Handle(It.IsAny<DomainEventNotification<TDomainEvent>>(),
                It.IsAny<CancellationToken>()))
            .Returns<DomainEventNotification<TDomainEvent>, CancellationToken>(
                (notification, _) =>
                {
                    callback(notification.DomainEvent);
                    return Task.CompletedTask;
                }));

    public static IServiceCollection MockConfig<TConfig>(this IServiceCollection services,
        TConfig? config = null) where TConfig : class, new() =>
        services.AddSingleton(Options.Options.Create(config ?? new()));

    public static IServiceCollection MockLogger<TContext>(this IServiceCollection services,
        Action<string> callback, Action<Exception>? errorCallback = null, LogLevel? logLevel = null) =>
        services.Mock<ILogger<TContext>>(x => x.Capture(callback, errorCallback, logLevel));
    
    public static IServiceCollection AlwaysValid<TRequest>(this IServiceCollection services)
        where TRequest : class {
        services.RemoveAll(typeof(IValidator<TRequest>));
        var validator = new Mock<IValidator<TRequest>>();
        var validResult = new ValidationResult();
        validator.Setup(x => x.Validate(It.IsAny<IValidationContext>())).Returns(validResult);
        validator.Setup(x => x.Validate(It.IsAny<TRequest>())).Returns(validResult);
        validator.Setup(x => x.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validResult);
        validator.Setup(x => x.ValidateAsync(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validResult);
        return services.AddMock(validator);
    }
    
    public static IServiceCollection HasErrors<TRequest>(this IServiceCollection services,
        params ValidationFailure[] errors)
        where TRequest : class {
        if (errors.Length < 1) {
            throw new ArgumentException("Please define at least 1 error.", nameof(errors));
        }
        
        services.RemoveAll(typeof(IValidator<TRequest>));
        var invalidResult = new ValidationResult(errors);
        var validator = new Mock<IValidator<TRequest>>();
        void AddErrors(IValidationContext context) =>
            Array.ForEach(errors, ((ValidationContext<TRequest>)context).AddFailure);
        validator.Setup(x => x.Validate(It.IsAny<IValidationContext>()))
            .Callback(AddErrors).Returns(invalidResult);
        validator.Setup(x => x.Validate(It.IsAny<TRequest>())).Returns(invalidResult);
        validator.Setup(x => x.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .Callback((IValidationContext ctx, CancellationToken _) => AddErrors(ctx))
            .ReturnsAsync(() => invalidResult);
        validator.Setup(x => x.ValidateAsync(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => invalidResult);
        return services.AddMock(validator);
    }
    
    public static ValidationFailure Error(string propertyName, string? message = null) => new() {
        PropertyName = propertyName, ErrorMessage = message ?? Generator.Lorem.Sentence()
    };
}
