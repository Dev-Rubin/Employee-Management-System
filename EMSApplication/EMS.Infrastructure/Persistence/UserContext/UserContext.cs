using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EMS.Infrastructure.Persistence.UserContext
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
        public int? UserId => int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value,out var id) ? id : null;
        public string? UserName => User?.Identity?.Name;
        public string? Role => User?.FindFirst(ClaimTypes.Role)?.ToString();
        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    }

}
