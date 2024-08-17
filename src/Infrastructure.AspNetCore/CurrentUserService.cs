using System.Security.Claims;
using DevKit.Application.Ports;
using Microsoft.AspNetCore.Http;

namespace DevKit.Infrastructure.AspNetCore;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string UserId =>
        httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
        string.Empty;
}
