using System.Reflection;
using DevKit.Application.Attributes;
using DevKit.Application.Ports;
using DevKit.Domain.Exceptions;

namespace DevKit.Application.Behaviour;

public sealed class AuthorizationBehaviour<TRequest, TResponse>(
    ICurrentUserService currentUserService,
    IIdentityService identityService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken) {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>().ToArray();
        if (!authorizeAttributes.Any()) return await next();

        // Must be authenticated user
        string userId = currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthenticatedException();

        // Role-based authorization
        var authorizeAttributesWithRoles =
            authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Roles)).ToArray();
        if (authorizeAttributesWithRoles.Any()) {
            foreach (string[] roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(','))) {
                bool authorized = false;
                foreach (string role in roles) {
                    bool isInRole = await identityService.IsInRoleAsync(userId, role.Trim());
                    if (isInRole) {
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
        if (authorizeAttributesWithPolicies.Any()) {
            foreach (string policy in authorizeAttributesWithPolicies.Select(a => a.Policy)) {
                bool authorized = await identityService.AuthorizeAsync(userId, policy);
                if (!authorized) throw new ForbiddenAccessException();
            }
        }

        // User is authorized / authorization not required
        return await next();
    }
}
