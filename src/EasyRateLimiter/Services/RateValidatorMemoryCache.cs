using EasyRateLimiter.Helpers;
using EasyRateLimiter.Options;
using EasyRateLimiter.Repositories;

namespace EasyRateLimiter.Services;

public class RateValidatorMemoryCache(
    MemoryCacheRepository repository,
    RateLimiterOptions options) : IRateValidator
{
    private readonly object _lock = new();

    public Task<bool> CheckRateLimitAsync(string ipOrClientId, string endpoint, long nowTicks,
        bool checkGlobalLimiting)
    {
        TimeSpan maxExpiryTime;
        long maxExpiryTicks;

        var key = checkGlobalLimiting ? ipOrClientId : $"{ipOrClientId}:{endpoint}";

        var isRuleMatch = checkGlobalLimiting
            ? options.Rules.Any(e => e.Endpoint == "*")
            : options.Rules.Any(e => e.Endpoint == endpoint);

        switch (isRuleMatch)
        {
            case true when checkGlobalLimiting:
                maxExpiryTime = options.Rules.Where(e => e.Endpoint == "*").Max(x => x.PeriodTimeSpan);
                maxExpiryTicks = options.Rules.Where(e => e.Endpoint == "*").Max(x => x.PeriodTicks);
                break;
            case true when !checkGlobalLimiting:
                maxExpiryTime = options.Rules.Where(e => e.Endpoint == endpoint).Max(x => x.PeriodTimeSpan);
                maxExpiryTicks = options.Rules.Where(e => e.Endpoint == endpoint).Max(x => x.PeriodTicks);
                break;
            default:
                return Task.FromResult(false);
        }

        List<long> cachedValues;
        lock (_lock)
        {
            var fetchedValues = repository.GetCacheEntriesAsync(key);
            cachedValues = fetchedValues ?? [];

            cachedValues.Add(nowTicks);
            if (fetchedValues is null)
            {
                repository.CreateCacheEntryAsync(key, cachedValues, maxExpiryTime);
            }
            else
            {
                cachedValues = cachedValues.Where(x => x > nowTicks - maxExpiryTicks).ToList();
                repository.UpdateCacheEntryAsync(key, cachedValues, maxExpiryTime);
            }
        }

        var isRuleViolated =
            RuleCheckHelper.RulesAreViolated(options.Rules, endpoint, cachedValues, nowTicks, checkGlobalLimiting);
        return Task.FromResult(isRuleViolated);
    }
}