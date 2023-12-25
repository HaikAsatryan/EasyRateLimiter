using System.Net;

namespace EasyRateLimiter.Helpers;

public static class IpAddressValidator
{
    public static bool IsValid(string? ipAddress)
    {
        return !string.IsNullOrWhiteSpace(ipAddress) && IPAddress.TryParse(ipAddress, out _);
    }

    public static bool IsValid(List<string>? ipAddresses)
    {
        if (ipAddresses == null || ipAddresses.Count == 0)
        {
            return false;
        }

        return ipAddresses.All(IsValid);
    }
}