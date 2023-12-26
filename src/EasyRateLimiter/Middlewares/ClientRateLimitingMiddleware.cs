using System.Net;
using EasyRateLimiter.Options;
using EasyRateLimiter.Repositories;
using EasyRateLimiter.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EasyRateLimiter.Middlewares;

public class ClientRateLimitingMiddleware(
    RequestDelegate next,
    ILogger<ClientRateLimitingMiddleware> logger,
    RateLimitingService rateLimitingService)
{
    public async Task InvokeAsync(HttpContext context, ICacheRepository repository, RateLimiterOptions options)
    {
        if (options.EnableClientIdRateLimiting == false)
        {
            logger.LogWarning("ClientRateLimitingMiddleware is disabled but still in the pipeline.");
            await next(context);
            return;
        }

        var headerName = options.ClientIdHeader;
        var clientId = context.Request.Headers[headerName!].ToString();
        if (string.IsNullOrWhiteSpace(clientId))
        {
            logger.LogWarning(
                "ClientRateLimitingMiddleware is enabled but no client id header has been recognized from http request context.");
            await next(context);
            return;
        }

        var endpoint = $"{context.Request.Method}:{context.Request.Path}";
        endpoint = endpoint.ToLower();


        if (await rateLimitingService.IsRateLimited(clientId, endpoint, false))
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