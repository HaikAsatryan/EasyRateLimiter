using EasyRateLimiter.Options;

namespace EasyRateLimiter.Helpers;

public static class RuleCheckHelper
{
    public static bool RulesAreViolated(List<RateLimitRule> rules, string endpoint, List<long> ticks,
        long nowTicks, bool globalRules)
    {
        ticks.Sort();
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
            var periodStart = nowTicks - rule.PeriodTicks;

            for (var i = ticks.Count - 1; i >= 0; i--)
            {
                if (ticks[i] < periodStart)
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
}