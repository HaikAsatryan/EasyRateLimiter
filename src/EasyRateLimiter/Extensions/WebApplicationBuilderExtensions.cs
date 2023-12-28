using EasyRateLimiter.Helpers;
using EasyRateLimiter.Options;
using EasyRateLimiter.Repositories;
using EasyRateLimiter.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EasyRateLimiter.Extensions;

public static class WebApplicationBuilderExtensions
{
    private static bool _rateLimiterRegistered;

    public static WebApplicationBuilder AddRateLimiter(this WebApplicationBuilder builder)
    {
        builder.CheckDuplicateRateLimiter();
        BindAndConfigureRateLimiterOptions(builder);
        builder.RegisterRateLimiterServices();
        return builder;
    }

    public static WebApplicationBuilder AddRateLimiter(this WebApplicationBuilder builder,
        Action<RateLimiterOptions> setupAction)
    {
        builder.CheckDuplicateRateLimiter();
        BindAndConfigureRateLimiterOptions(builder, setupAction);
        builder.RegisterRateLimiterServices();
        return builder;
    }

    public static WebApplicationBuilder AddDistributedRateLimiter(this WebApplicationBuilder builder)
    {
        builder.CheckDuplicateRateLimiter();

        var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
        builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString!));

        BindAndConfigureRateLimiterOptions(builder);
        builder.RegisterDistributedRateLimiterServices();

        return builder;
    }

    public static WebApplicationBuilder AddDistributedRateLimiter(this WebApplicationBuilder builder,
        Action<RateLimiterOptions> setupAction)
    {
        builder.CheckDuplicateRateLimiter();

        var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
        builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString!));
        BindAndConfigureRateLimiterOptions(builder, setupAction);

        builder.RegisterDistributedRateLimiterServices();

        return builder;
    }


    public static WebApplicationBuilder AddDistributedRateLimiter(this WebApplicationBuilder builder,
        string redisConnectionString)
    {
        builder.CheckDuplicateRateLimiter();
        builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
        BindAndConfigureRateLimiterOptions(builder);
        builder.RegisterDistributedRateLimiterServices();


        return builder;
    }

    public static WebApplicationBuilder AddDistributedRateLimiter(this WebApplicationBuilder builder,
        string redisConnectionString, Action<RateLimiterOptions> setupAction)
    {
        builder.CheckDuplicateRateLimiter();
        builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
        BindAndConfigureRateLimiterOptions(builder, setupAction);
        builder.RegisterDistributedRateLimiterServices();
        return builder;
    }

    private static WebApplicationBuilder RegisterRateLimiterServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<RateLimitingService>();
        builder.Services.AddSingleton<IRateValidator, RateValidatorMemoryCache>();
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<MemoryCacheRepository>();
        return builder;
    }

    private static WebApplicationBuilder RegisterDistributedRateLimiterServices(this WebApplicationBuilder builder)
    {
        var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
        builder.Services.AddSingleton<RateLimitingService>();
        builder.Services.AddSingleton<RedisLuaRepository>();
        builder.Services.AddSingleton<IRateValidator, RateValidatorRedis>();
        return builder;
    }

    private static RateLimiterOptions BindAndConfigureRateLimiterOptions(WebApplicationBuilder builder,
        Action<RateLimiterOptions> setupAction)
    {
        var options = new RateLimiterOptions();
        builder.Configuration.GetSection("RateLimiter").Bind(options, c => c.BindNonPublicProperties = true);
        options.Rules.ForEach(rule => rule.PeriodTicks = TimeParser.ToTicks(rule.Period));
        options.Rules.ForEach(rule => rule.PeriodTimeSpan = TimeParser.ToTimeSpan(rule.Period));
        options.Rules.ForEach(rule => rule.PeriodSeconds = (int)rule.PeriodTimeSpan.TotalSeconds);
        setupAction.Invoke(options);
        OptionsValidator.Validate(options);
        options.EndpointWhitelist = EndpointListHelper.Build(options.IpWhitelist);
        builder.Services.AddSingleton(options);


        return options;
    }

    private static RateLimiterOptions BindAndConfigureRateLimiterOptions(WebApplicationBuilder builder)
    {
        var options = new RateLimiterOptions();
        builder.Configuration.GetSection("RateLimiter").Bind(options, c => c.BindNonPublicProperties = true);
        options.Rules.ForEach(rule => rule.PeriodTicks = TimeParser.ToTicks(rule.Period));
        options.Rules.ForEach(rule => rule.PeriodTimeSpan = TimeParser.ToTimeSpan(rule.Period));
        options.Rules.ForEach(rule => rule.PeriodSeconds = (int)rule.PeriodTimeSpan.TotalSeconds);
        OptionsValidator.Validate(options);
        options.EndpointWhitelist = EndpointListHelper.Build(options.EndpointWhitelist);
        var newRules = new List<RateLimitRule>();
        foreach (var rule in options.Rules)
        {
            if (rule.Endpoint != "*" && rule.Endpoint.StartsWith("*"))
            {
                var newEndpoints = EndpointListHelper.Build(rule.Endpoint);
                newRules.AddRange(newEndpoints.Select(newEndpoint => new RateLimitRule
                {
                    Endpoint = newEndpoint, Period = rule.Period, Limit = rule.Limit,
                    PeriodTicks = rule.PeriodTicks
                }));
            }
            else
            {
                newRules.Add(rule);
            }
        }

        options.Rules = newRules;
        builder.Services.AddSingleton(options);
        return options;
    }

    private static WebApplicationBuilder CheckDuplicateRateLimiter(this WebApplicationBuilder builder)
    {
        if (_rateLimiterRegistered)
        {
            throw new InvalidOperationException("A RateLimiter is already configured. Cannot add another one.");
        }

        _rateLimiterRegistered = true;
        return builder;
    }
}