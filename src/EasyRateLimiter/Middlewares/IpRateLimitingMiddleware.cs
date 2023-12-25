using System.Diagnostics;
using System.Net;
using EasyRateLimiter.Options;
using EasyRateLimiter.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using EasyRateLimiter.Helpers;

namespace EasyRateLimiter.Middlewares;

public class IpRateLimitingMiddleware(
    RequestDelegate next,
    ILogger<IpRateLimitingMiddleware> logger,
    RateLimitingService rateLimitingService)
{
    public async Task InvokeAsync(HttpContext context, ICacheRepository repository, RateLimiterOptions options)
    {
        var sp = new Stopwatch();
        sp.Start();
        if (options.EnableIpRateLimiting == false)
        {
            logger.LogWarning("IpRateLimitingMiddleware is disabled but still in the pipeline.");
            await next(context);
            return;
        }


        var ipAddress = context.Request.TryGetClientIpAddress(options.IpHeader!);
        var endpoint = $"{context.Request.Method}:{context.Request.Path}";
        endpoint = endpoint.ToLower();

        if (await rateLimitingService.IsRateLimited(ipAddress, endpoint, true))
        {
            if (options.HttpStatusCode == 0)
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            }

            context.Response.StatusCode = options.HttpStatusCode;

            return;
        }

        sp.Stop();
        logger.LogInformation("IpRateLimiter middleware validated rate in {TotalMicroseconds} microseconds",
            sp.Elapsed.TotalMilliseconds); //todo only for testing delete at the end
        await next(context);
    }
}