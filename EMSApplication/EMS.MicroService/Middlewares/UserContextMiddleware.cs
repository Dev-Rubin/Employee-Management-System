using System.Security.Claims;

namespace EMS.MicroService.Middlewares
{
    public class UserContextMiddleware
    {
        private readonly RequestDelegate _next;

        public UserContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userIdHeader),
                    new Claim(ClaimTypes.Name, "DemoUser")
                 };

                var identity = new ClaimsIdentity(claims, "jwt");
                httpContext.User = new ClaimsPrincipal(identity);
            }

            await _next(httpContext);
        }
    }


}
