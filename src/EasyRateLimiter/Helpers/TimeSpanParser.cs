namespace EasyRateLimiter.Helpers;

using System;

public static class TimeSpanParser
{
    public static TimeSpan Parse(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentException("Expression cannot be null or empty.");
        }

        var unit = char.ToLower(expression[^1]);
        if (!int.TryParse(expression[..^1], out var value))
        {
            throw new ArgumentException("Invalid numeric value in time span expression.");
        }

        return unit switch
        {
            's' => TimeSpan.FromSeconds(value),
            'm' => TimeSpan.FromMinutes(value),
            'h' => TimeSpan.FromHours(value),
            'd' => TimeSpan.FromDays(value),
            _ => throw new ArgumentException("Invalid time unit in expression.")
        };
    }
}