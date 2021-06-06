using System.Security.Claims;
using FM.Application.Ports;
using Microsoft.AspNetCore.Http;

namespace FM.Infrastructure.AspNetCore
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserId => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                                string.Empty;
    }
}
