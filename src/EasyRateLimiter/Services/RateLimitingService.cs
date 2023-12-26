using EasyRateLimiter.Helpers;
using EasyRateLimiter.Options;

namespace EasyRateLimiter.Services;

public class RateLimitingService(RateLimiterOptions options, IRateValidator validator)
{
    public async Task<bool> IsRateLimited(string ipOrClientId, string endpoint, bool isIpRateLimiting)
    {
        var now = DateTime.UtcNow;

        if (WhitelistHelper.IsWhitelisted(ipOrClientId, endpoint, isIpRateLimiting, options))
        {
            return false;
        }

        var isSpecificRateLimitedTask = validator.CheckRateLimitAsync(ipOrClientId, endpoint, now, false);
        var isGloballyRateLimitedTask = validator.CheckRateLimitAsync(ipOrClientId, endpoint, now, true);

        await Task.WhenAll(isSpecificRateLimitedTask, isGloballyRateLimitedTask);

        return isSpecificRateLimitedTask.Result || isGloballyRateLimitedTask.Result;
    }
}