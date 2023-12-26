namespace EasyRateLimiter.Options;

public class RateLimiterOptions
{
    public bool? EnableIpRateLimiting { get; set; }

    public bool? EnableClientIdRateLimiting { get; set; }

    public string? IpHeader { get; set; }

    public string? ClientIdHeader { get; set; }

    public int HttpStatusCode { get; set; } = 429;

    public List<string>? IpWhitelist { get; set; }

    public List<string>? EndpointWhitelist { get; set; }

    public List<string>? ClientWhitelist { get; set; }

    public List<RateLimitRule> Rules { get; set; } = null!;
}