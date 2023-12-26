using EasyRateLimiter.Helpers;
using EasyRateLimiter.Options;
using EasyRateLimiter.Repositories;

namespace EasyRateLimiter.Services;

public class RateValidatorWithLock(
    ICacheRepository repository,
    RateLimiterOptions options) : IRateValidator
{
    private readonly object _lock = new();

    public Task<bool> CheckRateLimitAsync(string ipOrClientId, string endpoint, DateTime now,
        bool checkGlobalLimiting)
    {
        TimeSpan maxExpiryTime;

        var key = checkGlobalLimiting ? ipOrClientId : $"{ipOrClientId}:{endpoint}";

        var isRuleMatch = checkGlobalLimiting
            ? options.Rules.Any(e => e.Endpoint == "*")
            : options.Rules.Any(e => e.Endpoint == endpoint);

        switch (isRuleMatch)
        {
            case true when checkGlobalLimiting:
                maxExpiryTime = options.Rules.Where(e => e.Endpoint == "*").Max(x => x.PeriodTimeSpan);
                break;
            case true when !checkGlobalLimiting:
                maxExpiryTime = options.Rules.Where(e => e.Endpoint == endpoint).Max(x => x.PeriodTimeSpan);
                break;
            default:
                return Task.FromResult(false);
        }

        List<DateTime> cachedValues;
        lock (_lock)
        {
            var fetchedValues = repository.GetCacheEntriesAsync(key).Result;
            cachedValues = fetchedValues ?? [];

            cachedValues.Add(now);
            if (fetchedValues is null)
            {
                repository.CreateCacheEntryAsync(key, cachedValues, maxExpiryTime).Wait();
            }
            else
            {
                cachedValues = cachedValues.Where(x => x > now - maxExpiryTime).ToList();
                repository.UpdateCacheEntryAsync(key, cachedValues, maxExpiryTime).Wait();
            }
        }

        var isRuleViolated =
            RuleCheckHelper.RulesAreViolated(options.Rules, endpoint, cachedValues, now, checkGlobalLimiting);
        return Task.FromResult(isRuleViolated);
    }
}