using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FM.Application.Attributes;
using FM.Application.Ports;
using FM.Domain.Exception;
using MediatR;

namespace FM.Application.Behaviour
{
    public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IIdentityService _identityService;

        public AuthorizationBehaviour(
            ICurrentUserService currentUserService,
            IIdentityService identityService)
        {
            _currentUserService = currentUserService;
            _identityService = identityService;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>().ToArray();
            if (authorizeAttributes.Any())
            {
                // Must be authenticated user
                string userId = _currentUserService.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                    throw new UnauthenticatedException();

                // Role-based authorization
                var authorizeAttributesWithRoles =
                    authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Roles)).ToArray();
                if (authorizeAttributesWithRoles.Any())
                {
                    foreach (string[] roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(',')))
                    {
                        bool authorized = false;
                        foreach (string role in roles)
                        {
                            bool isInRole = await _identityService.IsInRoleAsync(userId, role.Trim());
                            if (isInRole)
                            {
                                authorized = true;
                                break;
                            }
                        }

                        // Must be a member of at least one role in roles
                        if (!authorized) throw new ForbiddenAccessException();
                    }
                }

                // Policy-based authorization
                var authorizeAttributesWithPolicies =
                    authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Policy)).ToArray();
                if (authorizeAttributesWithPolicies.Any())
                {
                    foreach (string policy in authorizeAttributesWithPolicies.Select(a => a.Policy))
                    {
                        bool authorized = await _identityService.AuthorizeAsync(userId, policy);
                        if (!authorized) throw new ForbiddenAccessException();
                    }
                }
            }

            // User is authorized / authorization not required
            return await next();
        }
    }
}
