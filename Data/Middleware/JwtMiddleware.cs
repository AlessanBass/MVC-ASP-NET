using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MvcMovie.Data.Auth.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtMiddleware(RequestDelegate next, IHttpContextAccessor httpContextAccessor)
        {
            _next = next;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("JWT_TOKEN");

            if (!string.IsNullOrEmpty(token))
            {
                context.Response.Cookies.Append("JWT_TOKEN", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = context.Request.IsHttps,
                    SameSite = SameSiteMode.Strict
                });
            }
            await _next(context);
        }
    }
}
