using EasyRateLimiter.Helpers;
using EasyRateLimiter.Options;

namespace EasyRateLimiter.Services;

public class RateLimitingService(RateLimiterOptions options, IRateValidator validator)
{
    public async Task<bool> IsRateLimited(string ipOrClientId, string endpoint, bool isIpRateLimiting, long nowTicks)
    {

        if (WhitelistHelper.IsWhitelisted(ipOrClientId, endpoint, isIpRateLimiting, options))
        {
            return false;
        }

        var isSpecificRateLimited = await validator.CheckRateLimitAsync(ipOrClientId, endpoint, nowTicks, false);
        var isGloballyRateLimited = await validator.CheckRateLimitAsync(ipOrClientId, endpoint, nowTicks, true);


        return isSpecificRateLimited || isGloballyRateLimited;
    }
}