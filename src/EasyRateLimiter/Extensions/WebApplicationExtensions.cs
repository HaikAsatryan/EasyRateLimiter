using EasyRateLimiter.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace EasyRateLimiter.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseClientRateLimiter(this WebApplication app)
    {
        app.UseMiddleware<ClientRateLimitingMiddleware>();
        return app;
    }

    public static WebApplication UseIpRateLimiter(this WebApplication app)
    {
        app.UseMiddleware<IpRateLimitingMiddleware>();
        return app;
    }
}