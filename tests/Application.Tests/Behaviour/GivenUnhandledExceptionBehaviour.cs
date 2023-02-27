using System.Reflection;
using DevKit.Application.Attributes;
using DevKit.Application.Behaviour;
using DevKit.Domain.Exceptions;

namespace DevKit.Application.Tests.Behaviour;

using static Testing;

[TestFixture]
[Parallelizable(ParallelScope.Children)]
public class GivenUnhandledExceptionBehaviour
{
    private static IServiceProvider Setup<TRequest>(Action<string> logCallback,
        Action<Exception> errorCallback, SyncCommandCallback<TRequest> callback)
        where TRequest : IRequest<Unit> => ConfigureServices(services => services
        .MockLogger<UnhandledExceptionBehaviour<TRequest, Unit>>(logCallback, errorCallback, LogLevel.Error)
        .MockHandler(callback));

    [Test]
    [TestCaseSource(nameof(AllBusinessExceptions))]
    public void WhenHandlerThrowsBusinessExceptionThenItWillNotBeLogged(Type exceptionType) {
        var logMessages = new List<string>();
        var exceptions = new List<Exception>();
        var provider = Setup<Command.Business>(logMessages.Add, exceptions.Add, request =>
        {
            var exc = (Exception?)Activator.CreateInstance(request.ExceptionType) ?? new Exception();
            throw exc;
        });
        var mediator = provider.Resolve<IMediator>();
        var exception = Assert.ThrowsAsync(exceptionType,
            async () => await mediator.Send(new Command.Business(exceptionType)));

        Assert.That(exception, Is.Not.Null);
        Assert.That(logMessages, Is.Empty);
        Assert.That(exceptions, Is.Empty);
    }

    [Test]
    public void WhenHandlerThrowsNonBusinessExceptionThenItWillBeLoggedAsError() {
        string logMessage = string.Empty;
        Exception? logException = null;
        var provider = Setup<Command.NonBusiness>(log => logMessage = log, exc => logException = exc,
            _ => throw new UnauthorizedAccessException("intended"));
        var mediator = provider.Resolve<IMediator>();

        var exception =
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => mediator.Send(new Command.NonBusiness()));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Is.EqualTo("intended"));
        string className =
            $"{nameof(GivenUnhandledExceptionBehaviour)}{nameof(Command)}{nameof(Command.NonBusiness)}";
        Assert.That(logMessage, Does.StartWith($"Unhandled exception for request {className}"));
        Assert.That(logException, Is.Not.Null.And.SameAs(exception));
    }

    public static IEnumerable<Type> AllBusinessExceptions
    {
        get
        {
            var baseType = typeof(BusinessApplicationException);
            var domainAssembly = baseType.GetTypeInfo().Assembly;
            var infraAssembly = typeof(AuthorizeAttribute).GetTypeInfo().Assembly;
            return new[] { domainAssembly, infraAssembly }.SelectMany(a => a.GetExportedTypes())
                .Where(t =>
                    t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t) &&
                    t.HasParameterLessConstructor());
        }
    }

    public struct Command
    {
        public record Business(Type ExceptionType) : IRequest;

        public record NonBusiness : IRequest;
    }
}
