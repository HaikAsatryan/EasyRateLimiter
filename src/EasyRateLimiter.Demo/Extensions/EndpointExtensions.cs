using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace EasyRateLimiter.Demo.Extensions;

public static class EndpointExtensions
{
    public static void MapCustomEndpoints(this WebApplication app)
    {
        app.MapPingApi()
            .MapClientIdCheckEndpoint()
            .MapAllRequestResponseHeadersApi();
    }

    private static WebApplication MapClientIdCheckEndpoint(this WebApplication app)
    {
        app.MapGet("/above-board/client-id-check",
                ([FromHeader] string clientId, HttpContext context) => context.Request.Headers["ClientId"])
            .WithTags("Above Board");

        return app;
    }

    private static WebApplication MapPingApi(this WebApplication app)
    {
        app.MapGet("/above-board/ping", () => "pong").WithTags("Above Board").RequireRateLimiting("SlidingWindowLimiter");
        return app;
    }


    private static WebApplication MapAllRequestResponseHeadersApi(this WebApplication app)
    {
        app.MapGet("/above-board/headers", (HttpContext context) =>
        {
            var requestHeaders = context.Request.Headers
                .ToDictionary(h => h.Key, h => h.Value.ToString());
            var responseHeaders = context.Response.Headers
                .ToDictionary(h => h.Key, h => h.Value.ToString());

            var connectionInfo = new
            {
                RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
                RemotePort = context.Connection.RemotePort,
                LocalIpAddress = context.Connection.LocalIpAddress?.ToString(),
                LocalPort = context.Connection.LocalPort,
                ClientCertificate = context.Connection.ClientCertificate?.ToString()
            };

            var headersInfo = new
            {
                Connection = connectionInfo,
                RequestHeaders = requestHeaders,
                ResponseHeaders = responseHeaders,
            };

            return Results.Json(headersInfo, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }).WithTags("Above Board");

        return app;
    }
}