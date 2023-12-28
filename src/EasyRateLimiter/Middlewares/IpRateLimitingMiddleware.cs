using System.Net;
using EasyRateLimiter.Options;
using EasyRateLimiter.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using EasyRateLimiter.Helpers;
using EasyRateLimiter.Services;

namespace EasyRateLimiter.Middlewares;

public class IpRateLimitingMiddleware(
    RequestDelegate next,
    ILogger<IpRateLimitingMiddleware> logger,
    RateLimitingService rateLimitingService)
{
    public async Task InvokeAsync(HttpContext context, MemoryCacheRepository repository, RateLimiterOptions options)
    {
        var nowTicks = DateTime.UtcNow.Ticks;
        if (options.EnableIpRateLimiting == false)
        {
            logger.LogWarning("IpRateLimitingMiddleware is disabled but still in the pipeline.");
            await next(context);
            return;
        }


        var ipAddress = context.Request.TryGetClientIpAddress(options.IpHeader!);
        var endpoint = $"{context.Request.Method}:{context.Request.Path}";
        endpoint = endpoint.ToLower();

        if (await rateLimitingService.IsRateLimited(ipAddress, endpoint, true, nowTicks))
        {
            if (options.HttpStatusCode == 0)
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            }

            context.Response.StatusCode = options.HttpStatusCode;

            return;
        }

        await next(context);
    }
}