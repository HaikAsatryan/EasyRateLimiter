using EasyRateLimiter.Options;

namespace EasyRateLimiter.Helpers;

public static class OptionsValidator
{
    public static void Validate(RateLimiterOptions options)
    {
        if (options.EnableIpRateLimiting == null)
            throw new InvalidOperationException("EnableIpRateLimiting option is required.");

        if (options.EnableClientIdRateLimiting == null)
            throw new InvalidOperationException("EnableClientIdRateLimiting option is required.");

        if (options.EnableClientIdRateLimiting == true && string.IsNullOrWhiteSpace(options.ClientIdHeader))
            throw new InvalidOperationException("ClientIdHeader is required when ClientId rate limiting is enabled.");

        if (options.Rules == null || options.Rules.Count == 0)
            throw new InvalidOperationException("At least one rule is required in Rules.");

        // the line below is commented, because sometime there are Ip addresses like this ::12:23 
        
        // if (options.IpWhitelist != null && options.IpWhitelist.Count > 0 &&
        //     !IpAddressValidator.IsValid(options.IpWhitelist))
        //     throw new InvalidOperationException("IpWhitelist contains invalid IP addresses.");

        if (options.HttpStatusCode != 0 && options.HttpStatusCode < 400 && options.HttpStatusCode > 599)
            throw new InvalidOperationException("HttpStatusCode must be between 400 and 599.");

        ValidateRules(options.Rules);
    }

    private static void ValidateRules(List<RateLimitRule> rules)
    {
        foreach (var rule in rules)
        {
            if (string.IsNullOrWhiteSpace(rule.Endpoint))
            {
                throw new ArgumentException("Endpoint is required.");
            }

            if (rule.PeriodTicks <= 0)
            {
                throw new ArgumentException("Period must be greater than zero.");
            }

            if (rule.Limit <= 0)
            {
                throw new ArgumentException("Limit must be greater than zero.");
            }
        }
    }
}