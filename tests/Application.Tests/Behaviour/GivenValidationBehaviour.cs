using FluentValidation;
using ValidationException = DevKit.Domain.Exceptions.ValidationException;

namespace DevKit.Application.Tests.Behaviour;

using static ApplicationDependencyExtensions;
using static Testing;

[TestFixture]
[Parallelizable(ParallelScope.Children)]
public class GivenValidationBehaviour
{
    private static IServiceProvider Setup(SetupService? setup = null) => ConfigureServices(services =>
        setup?.Invoke(services
            .MockHandler<Command.Validated>()
            .MockHandler<Command.Unvalidated>()));

    [Test]
    public void WhenValidationConfiguredAndFailThenValidationExceptionWillBeThrown() {
        var provider = Setup(services => services
            .HasErrors<Command.Validated>(Error(nameof(Command.Validated.Id))));
        var mediator = provider.Resolve<IMediator>();

        Assert.That(() => mediator.Send(new Command.Validated(default)),
            Throws.InstanceOf<ValidationException>());
    }

    [Test]
    public async Task WhenValidationConfiguredAndPassThenNoValidationExceptionWillBeThrown() {
        var provider = Setup(services => services
            .AlwaysValid<Command.Validated>());
        var mediator = provider.Resolve<IMediator>();

        await mediator.Send(new Command.Validated(1));
    }

    [Test]
    public async Task WhenNoValidationConfiguredThenNoValidationExceptionWillBeThrown() {
        var provider = Setup(services => services.MockHandler<Command.Unvalidated>());
        var mediator = provider.Resolve<IMediator>();

        await mediator.Send(new Command.Unvalidated());
    }

    public struct Command
    {
        public record Validated(int Id) : IRequest;

        public record Unvalidated : IRequest;

        public class Validator : AbstractValidator<Validated>
        {
            public Validator() => RuleFor(p => p.Id).GreaterThan(0);
        }
    }
}
