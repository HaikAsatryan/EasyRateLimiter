using System.Text.Json.Serialization;

namespace EasyRateLimiter.Options;

public class RateLimitRule
{
    public string Endpoint { get; set; } = null!;
    public string Period { get; set; } = null!;
    public int Limit { get; set; }

    [JsonIgnore] public long PeriodTicks { get; set; }
    [JsonIgnore] public TimeSpan PeriodTimeSpan { get; set; }
    [JsonIgnore] public int PeriodSeconds { get; set; }
}