using EasyRateLimiter.Options;

namespace EasyRateLimiter.Helpers;

public static class WhitelistHelper
{
    public static bool IsWhitelisted(string ipOrClientId, string endpoint, bool isIpRateLimiting, RateLimiterOptions options)
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