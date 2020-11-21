using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RateLimiter.RateLimit;

namespace RateLimiter.Middleware
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRateLimitPolicy _rateLimitPolicy;

        public RateLimitMiddleware(RequestDelegate next, IRateLimitPolicy rateLimitPolicy)
        {
            _next = next;
            _rateLimitPolicy = rateLimitPolicy;
        }

        public async Task Invoke(HttpContext context)
        {
            var header = context.Request.Headers;
            string user = header["User"];
            if (user == null || await _rateLimitPolicy.DoesThrottle(user))
            {
                context.Response.StatusCode = (int) HttpStatusCode.TooManyRequests; 
                return;
            }

            await _next(context);
        }
    }

    public static class RateLimiterExtensions
    {
        public static IApplicationBuilder UseRateLimiter(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitMiddleware>();
        }
    }
}