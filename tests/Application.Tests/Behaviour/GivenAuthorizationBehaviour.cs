using DevKit.Application.Attributes;
using DevKit.Application.Mocks;
using DevKit.Domain.Exceptions;

namespace DevKit.Application.Tests.Behaviour;

using static Testing;

[TestFixture]
[Parallelizable(ParallelScope.Children)]
public class GivenAuthorizationBehaviour
{
    private static IServiceProvider Setup<TRequest>(SetupService setup)
        where TRequest : IRequest<Unit> => Configure(services =>
    {
        services.MockHandler<TRequest>();
        services.Mock<ICurrentUserService>(x => x.AnonymousUser());
        services.Mock<IIdentityService>(x => x.NotGrantedForAnonymous());
        setup(services);
    });

    [Test]
    public void WhenUnauthenticatedUserExecutingGuardedCommandThenUnauthenticatedExceptionWillBeThrown() {
        var provider = Setup<Guarded.Command>(services => services
            .Mock<ICurrentUserService>(x => x.AnonymousUser()));
        var mediator = provider.Resolve<IMediator>();

        Assert.ThrowsAsync<UnauthenticatedException>(async () => await mediator.Send(new Guarded.ByRole()));
    }

    [Test]
    public void
        WhenAuthenticatedUserButNotInRoleExecutingGuardedCommandThenForbiddenAccessExceptionWillBeThrown() {
        var provider = Setup<Guarded.ByRole>(services => services
            .Mock<ICurrentUserService>(x => x.AuthenticatedUser())
            .Mock<IIdentityService>(x => x.NotInRole()));
        var mediator = provider.Resolve<IMediator>();

        Assert.ThrowsAsync<ForbiddenAccessException>(async () => await mediator.Send(new Guarded.ByRole()));
    }

    [Test]
    public async Task WhenAuthenticatedUserAndInRoleExecutingGuardedCommandThenItWillBeAllowed() {
        var provider = Setup<Guarded.ByRole>(services => services
            .Mock<ICurrentUserService>(x => x.AuthenticatedUser())
            .Mock<IIdentityService>(x => x.GrantedByRole()));
        var mediator = provider.Resolve<IMediator>();

        await mediator.Send(new Guarded.ByRole());
    }

    [Test]
    public void
        WhenAuthenticatedUserButNoPolicyAccessExecutesGuardedCommandThenForbiddenAccessExceptionWillBeThrown() {
        var provider = Setup<Guarded.ByPolicy>(services => services
            .Mock<ICurrentUserService>(x => x.AuthenticatedUser())
            .Mock<IIdentityService>(x => x.NotInPolicy()));
        var mediator = provider.Resolve<IMediator>();

        Assert.ThrowsAsync<ForbiddenAccessException>(() => mediator.Send(new Guarded.ByPolicy()));
    }

    [Test]
    public async Task WhenAuthenticatedUserHavingPolicyAccessExecutesGuardedCommandThenItWillBeAllowed() {
        var provider = Setup<Guarded.ByPolicy>(services => services
            .Mock<ICurrentUserService>(x => x.AuthenticatedUser())
            .Mock<IIdentityService>(x => x.GrantedByPolicy()));
        var mediator = provider.Resolve<IMediator>();

        await mediator.Send(new Guarded.ByPolicy());
    }

    public struct Guarded
    {
        [Authorize]
        public record Command : IRequest;

        [Authorize(Roles = "Role")]
        public record ByRole : IRequest;

        [Authorize(Policy = "Policy")]
        public record ByPolicy : IRequest;
    }
}
