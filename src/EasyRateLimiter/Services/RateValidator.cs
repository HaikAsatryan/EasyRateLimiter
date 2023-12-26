using EasyRateLimiter.Helpers;
using EasyRateLimiter.Options;
using EasyRateLimiter.Repositories;

namespace EasyRateLimiter.Services;

public class RateValidator(
    ICacheRepository repository,
    RateLimiterOptions options) : IRateValidator
{

    public async Task<bool> CheckRateLimitAsync(string ipOrClientId, string endpoint, DateTime now,
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
                return false;
        }

        List<DateTime> cachedValues;
        
        var fetchedValues = await repository.GetCacheEntriesAsync(key);
        cachedValues = fetchedValues ?? [];

        cachedValues.Add(now);
        if (fetchedValues is null)
        {
            await repository.CreateCacheEntryAsync(key, cachedValues, maxExpiryTime);
        }
        else
        {
            cachedValues = cachedValues.Where(x => x > now - maxExpiryTime).ToList();
            await repository.UpdateCacheEntryAsync(key, cachedValues, maxExpiryTime); }


        return RuleCheckHelper.RulesAreViolated(options.Rules, endpoint, cachedValues, now, checkGlobalLimiting);
    }
}