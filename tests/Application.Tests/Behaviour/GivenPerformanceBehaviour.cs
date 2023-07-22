using DevKit.Application.Behaviour;

namespace DevKit.Application.Tests.Behaviour;

using static Testing;

[TestFixture]
[Parallelizable(ParallelScope.Children)]
public sealed class GivenPerformanceBehaviour
{
    private const int MaxLongRunningTask = 1000;

    private static IServiceProvider Setup(Action<string> logCallback, CommandCallback<Command> callback) =>
        Configure(services => services
            .MockHandler(callback)
            .MockLogger<PerformanceBehaviour<Command, Unit>>(logCallback, logLevel: LogLevel.Warning)
            .MockConfig(new GeneralConfig { MaxLongRunningTask = MaxLongRunningTask }));

    [Test]
    public async Task WhenRequestTakesLongerThanMaximumLongTaskThenItWillNotBeLoggedAsWarning() {
        string logMessage = string.Empty;
        var bootstrapper = Setup(log => logMessage = log,
            async (_, token) => await Task.Delay(MaxLongRunningTask + 100, token));
        var mediator = bootstrapper.Resolve<IMediator>();

        await mediator.Send(new Command());

        Assert.That(logMessage,
            Does.StartWith($"Long running request: {nameof(GivenPerformanceBehaviour)}{nameof(Command)}"));
    }

    [Test]
    public async Task WhenRequestTakesLessThanMaximumLongTaskThenItWillNotBeLoggedAsWarning() {
        string logMessage = string.Empty;
        var provider = Setup(log => logMessage = log,
            (_, token) => Task.Delay(MaxLongRunningTask / 2, token));
        var mediator = provider.Resolve<IMediator>();

        await mediator.Send(new Command());

        Assert.That(logMessage, Is.Empty);
    }

    public record Command : IRequest;
}
