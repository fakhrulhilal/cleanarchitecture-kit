namespace DevKit.Application.Tests.Behaviour;

using static Testing;

[TestFixture]
[Parallelizable(ParallelScope.Children)]
public class GivenPerformanceBehaviour
{
    private const int MaxLongRunningTask = 1000;

    private Startup Setup(Action<string> logCallback, Func<Command, CancellationToken, Task> handler) {
        var logger = new Mock<ILogger<Command>>();
        logger.CaptureLog(logCallback, expectedLogLevel: LogLevel.Warning);
        return ConfigureServices(services => services
            .AddMediatRHandler(handler)
            .AddTransient(_ => logger.Object)
            .AddSingleton(_ => new GeneralConfig { MaxLongRunningTask = MaxLongRunningTask }));
    }

    [Test]
    public async Task WhenRequestTakesLongerThanMaximumLongTaskThenItWillNotBeLoggedAsWarning() {
        string logMessage = string.Empty;
        var bootstrapper = Setup(log => logMessage = log,
            (_, token) => Task.Delay(MaxLongRunningTask + 100, token));
        var mediator = bootstrapper.GetService<IMediator>();

        await mediator.Send(new Command());

        Assert.That(logMessage,
            Does.StartWith($"Long running request: {nameof(GivenPerformanceBehaviour)}{nameof(Command)}"));
    }

    [Test]
    public async Task WhenRequestTakesLessThanMaximumLongTaskThenItWillNotBeLoggedAsWarning() {
        string logMessage = string.Empty;
        var bootstrapper = Setup(log => logMessage = log,
            (_, token) => Task.Delay(MaxLongRunningTask / 2, token));
        var mediator = bootstrapper.GetService<IMediator>();

        await mediator.Send(new Command());

        Assert.That(logMessage, Is.Empty);
    }

    public record Command : IRequest;
}
