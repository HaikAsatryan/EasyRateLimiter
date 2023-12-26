using EasyRateLimiter.Options;

namespace EasyRateLimiter.Helpers;

public static class RuleCheckHelper
{
    public static bool RulesAreViolated(List<RateLimitRule> rules, string endpoint, List<DateTime> values,
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
}