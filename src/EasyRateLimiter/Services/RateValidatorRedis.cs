using EasyRateLimiter.Helpers;
using EasyRateLimiter.Options;
using EasyRateLimiter.Repositories;

namespace EasyRateLimiter.Services;

public class RateValidatorRedis(
    RedisLuaRepository repository,
    RateLimiterOptions options) : IRateValidator
{
    public async Task<bool> CheckRateLimitAsync(string ipOrClientId, string endpoint, long nowTicks,
        bool checkGlobalLimiting)
    {
        int maxExpirySeconds;
        long maxExpiryTicks;


        var isRuleMatch = checkGlobalLimiting
            ? options.Rules.Any(e => e.Endpoint == "*")
            : options.Rules.Any(e => e.Endpoint == endpoint);

        switch (isRuleMatch)
        {
            case true when checkGlobalLimiting:
                maxExpirySeconds = options.Rules.Where(e => e.Endpoint == "*").Max(x => x.PeriodSeconds);
                maxExpiryTicks = options.Rules.Where(e => e.Endpoint == "*").Max(x => x.PeriodTicks);
                break;
            case true when !checkGlobalLimiting:
                maxExpirySeconds = options.Rules.Where(e => e.Endpoint == endpoint).Max(x => x.PeriodSeconds);
                maxExpiryTicks = options.Rules.Where(e => e.Endpoint == endpoint).Max(x => x.PeriodTicks);
                break;
            default:
                return false;
        }

        var key = checkGlobalLimiting ? ipOrClientId : $"{ipOrClientId}:{endpoint}";
        var cachedValues =
            await repository.GetAndUpdateCacheEntriesAsync(key, nowTicks, maxExpirySeconds, maxExpiryTicks);


        return RuleCheckHelper.RulesAreViolated(options.Rules, endpoint, cachedValues, nowTicks, checkGlobalLimiting);
    }
}