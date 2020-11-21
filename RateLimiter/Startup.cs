using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RateLimiter.Middleware;
using RateLimiter.RateLimit;
using RateLimiter.Repository.Redis;

namespace RateLimiter
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IRateLimitPolicy, TokenRateLimiter>();
            services.AddSingleton<RedisUserTokenRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRateLimiter();

            app.Run(async context =>
            {
                // TODO: write path through request to back-end server.
                await context.Response.WriteAsync("OK");
            });
        }
    }
}