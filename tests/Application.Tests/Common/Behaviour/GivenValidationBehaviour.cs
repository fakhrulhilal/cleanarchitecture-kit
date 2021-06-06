using System.Threading.Tasks;
using FluentValidation;
using FM.Tests;
using MediatR;
using NUnit.Framework;
using ValidationException = FM.Domain.Exception.ValidationException;

namespace FM.Application.Tests.Common.Behaviour
{
    using static Testing;

    [TestFixture]
    [Parallelizable(ParallelScope.Children)]
    public class GivenValidationBehaviour
    {
        private Startup _bootstrapper = new();

        [OneTimeSetUp]
        public void Setup() => _bootstrapper = ConfigureServices(services => services
            .AddMediatRHandler<Command.Validated>()
            .AddMediatRHandler<Command.Unvalidated>());

        [Test]
        public void WhenValidationConfiguredAndFailThenValidationExceptionWillBeThrown()
        {
            var mediator = _bootstrapper.GetService<IMediator>();

            Assert.That(() => mediator.Send(new Command.Validated(default)), Throws.InstanceOf<ValidationException>());
        }

        [Test]
        public async Task WhenValidationConfiguredAndPassThenNoValidationExceptionWillBeThrown()
        {
            var mediator = _bootstrapper.GetService<IMediator>();

            await mediator.Send(new Command.Validated(1));
        }

        [Test]
        public async Task WhenNoValidationConfiguredThenNoValidationExceptionWillBeThrown()
        {
            var mediator = _bootstrapper.GetService<IMediator>();

            await mediator.Send(new Command.Unvalidated());
        }

        public struct Command
        {
            public record Validated(int Id) : IRequest;
            public record Unvalidated : IRequest;
            public class Validator : AbstractValidator<Validated>
            {
                public Validator()
                {
                    RuleFor(p => p.Id).GreaterThan(0);
                }
            }
        }
    }
}
