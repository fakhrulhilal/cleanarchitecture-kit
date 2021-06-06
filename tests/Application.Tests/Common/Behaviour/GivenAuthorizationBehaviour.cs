using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FM.Application.Attributes;
using FM.Application.Ports;
using FM.Domain.Exception;
using FM.Tests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace FM.Application.Tests.Common.Behaviour
{
    using static Testing;

    [TestFixture]
    [Parallelizable(ParallelScope.Children)]
    public class GivenAuthorizationBehaviour
    {
        private const string Role = "Administrators";
        private const string Policy = "CanManage";
        private const string UserId = "Admin";

        private Startup Setup<TRequest>(string currentUser, Dictionary<string, string[]>? userRolesMapping = null,
            Dictionary<string, string[]>? userPolicyMapping = null)
            where TRequest : IRequest => ConfigureServices(services => services
            .AddMediatRHandler<TRequest>()
            .AddTransient(_ =>
            {
                var currentUserService = new Mock<ICurrentUserService>();
                currentUserService.SetupGet(x => x.UserId).Returns(() => currentUser);
                return currentUserService.Object;
            })
            .AddTransient(_ =>
            {
                var identityService = new Mock<IIdentityService>();
                userRolesMapping ??= new();
                userPolicyMapping ??= new();
                identityService.Setup(x => x.IsInRoleAsync(It.IsAny<string>(), It.IsAny<string>()).Result)
                    .Returns<string, string>((user, role) =>
                        userRolesMapping.ContainsKey(role) && userRolesMapping[role].Contains(user));
                identityService.Setup(x => x.AuthorizeAsync(It.IsAny<string>(), It.IsAny<string>()).Result)
                    .Returns<string, string>((user, policy) =>
                        userPolicyMapping.ContainsKey(policy) && userPolicyMapping[policy].Contains(user));
                return identityService.Object;
            }));

        [Test]
        public void WhenUnauthenticatedUserExecutingGuardedCommandThenUnauthenticatedExceptionWillBeThrown()
        {
            var bootstrapper = Setup<Guarded.Command>(string.Empty);
            var mediator = bootstrapper.GetService<IMediator>();

            Assert.ThrowsAsync<UnauthenticatedException>(() => mediator.Send(new Guarded.ByRole()));
        }

        [Test]
        public void WhenAuthenticatedUserButNotInRoleExecutingGuardedCommandThenForbiddenAccessExceptionWillBeThrown()
        {
            var bootstrapper = Setup<Guarded.ByRole>(UserId, new() { [Role] = new[] { "OtherUserId" } });
            var mediator = bootstrapper.GetService<IMediator>();

            Assert.ThrowsAsync<ForbiddenAccessException>(() => mediator.Send(new Guarded.ByRole()));
        }

        [Test]
        public async Task WhenAuthenticatedUserAndInRoleExecutingGuardedCommandThenItWillBeAllowed()
        {
            var bootstrapper = Setup<Guarded.ByRole>(UserId, new() { [Role] = new[] { UserId } });
            var mediator = bootstrapper.GetService<IMediator>();

            await mediator.Send(new Guarded.ByRole());
        }

        [Test]
        public void WhenAuthenticatedUserButNoPolicyAccessExecutesGuardedCommandThenForbiddenAccessExceptionWillBeThrown()
        {
            var bootstrapper = Setup<Guarded.ByPolicy>(UserId, userPolicyMapping: new() { [Policy] = Array.Empty<string>() });
            var mediator = bootstrapper.GetService<IMediator>();

            Assert.ThrowsAsync<ForbiddenAccessException>(() => mediator.Send(new Guarded.ByPolicy()));
        }

        [Test]
        public async Task WhenAuthenticatedUserHavingPolicyAccessExecutesGuardedCommandThenItWillBeAllowed()
        {
            var bootstrapper = Setup<Guarded.ByPolicy>(UserId, userPolicyMapping: new() { [Policy] = new[] { UserId } });
            var mediator = bootstrapper.GetService<IMediator>();

            await mediator.Send(new Guarded.ByPolicy());
        }

        public struct Guarded
        {
            [Authorize]
            public record Command : IRequest;
            [Authorize(Roles = Role)]
            public record ByRole : IRequest;
            [Authorize(Policy = Policy)]
            public record ByPolicy : IRequest;
        }
    }
}
