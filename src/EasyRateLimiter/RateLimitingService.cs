using EasyRateLimiter.Options;
using EasyRateLimiter.Repositories;

namespace EasyRateLimiter;

public class RateLimitingService(ICacheRepository repository, RateLimiterOptions options)
{
    public async Task<bool> IsRateLimited(string ipOrClientId, string endpoint, bool isIpRateLimiting)
    {
        var now = DateTime.UtcNow;

        if (IsWhitelisted(ipOrClientId, endpoint, isIpRateLimiting))
        {
            return false;
        }

        var isSpecificRateLimited = await CheckRateLimitAsync(ipOrClientId, endpoint, now, false);
        var isGloballyRateLimited = await CheckRateLimitAsync(ipOrClientId, endpoint, now, true);

        return isSpecificRateLimited || isGloballyRateLimited;
    }

    private async Task<bool> CheckRateLimitAsync(string ipOrClientId, string endpoint, DateTime now,
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

        List<DateTime>? cachedValues;
        try
        {
            cachedValues = await repository.GetCacheEntriesAsync(key);
        }
        catch
        {
            return false;
        }

        if (cachedValues is null)
        {
            var values = new List<DateTime> { now };
            try
            {
                await repository.CreateCacheEntryAsync(key, values, maxExpiryTime);
            }
            catch
            {
                return false;
            }

            return RulesAreViolated(options.Rules, endpoint, values, now, checkGlobalLimiting);
        }

        cachedValues.Add(now);
        cachedValues = cachedValues.Where(x => x > now - maxExpiryTime).ToList();
        try
        {
            await repository.UpdateCacheEntryAsync(key, cachedValues, maxExpiryTime);
        }
        catch
        {
            return false;
        }

        return RulesAreViolated(options.Rules, endpoint, cachedValues, now, checkGlobalLimiting);
    }

    private static bool RulesAreViolated(List<RateLimitRule> rules, string endpoint, List<DateTime> values,
        DateTime now, bool globalRules)
    {
        values.Sort();
        List<RateLimitRule> cleanedRules = [];

        foreach (var rule in rules)
        {
            if (globalRules && rule.Endpoint == "*")
            {
                cleanedRules.Add(rule);
            }
            else if (!globalRules && rule.Endpoint == endpoint)
            {
                cleanedRules.Add(rule);
            }
        }

        foreach (var rule in cleanedRules)
        {
            var count = 0;
            var periodStart = now - rule.PeriodTimeSpan;

            for (var i = values.Count - 1; i >= 0; i--)
            {
                if (values[i] < periodStart)
                {
                    break;
                }

                count++;

                if (count > rule.Limit)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsWhitelisted(string ipOrClientId, string endpoint, bool isIpRateLimiting)
    {
        if (isIpRateLimiting && options.IpWhitelist?.Contains(ipOrClientId) == true)
        {
            return true;
        }

        if (!isIpRateLimiting && options.ClientWhitelist?.Contains(ipOrClientId) == true)
        {
            return true;
        }

        if (options.EndpointWhitelist?.Contains(endpoint) == true)
        {
            return true;
        }

        return false;
    }
}