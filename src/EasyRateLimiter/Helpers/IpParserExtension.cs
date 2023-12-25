using Microsoft.AspNetCore.Http;

namespace EasyRateLimiter.Helpers
{
    public static class HttpRequestExtensions
    {
        public static string TryGetClientIpAddress(this HttpRequest request, string headerName)
        {
            string[] headersToCheck = [headerName, "X-Forwarded-For", "Forwarded"];

            foreach (var header in headersToCheck)
            {
                var ipAddress = ExtractIpAddressFromHeader(request, header);
                if (IpAddressValidator.IsValid(ipAddress))
                {
                    return ipAddress!;
                }
            }

            return request.HttpContext.Connection.RemoteIpAddress!.ToString();
        }

        private static string? ExtractIpAddressFromHeader(HttpRequest request, string headerName)
        {
            if (!request.Headers.TryGetValue(headerName, out var value))
            {
                return null;
            }

            if (headerName != "X-Forwarded-For")
            {
                return value.ToString().Split(',').Select(ip => ip.Trim()).LastOrDefault();
            }

            if (headerName == "Forwarded")
            {
                var forwardedValues = value.ToString().Split(',').Select(p => p.Trim());
                var latestProxy = forwardedValues.LastOrDefault();
                if (!string.IsNullOrWhiteSpace(latestProxy) && latestProxy.Length > 4)
                {
                    return latestProxy.Substring(4).Trim();
                }
            }

            return value.ToString();
        }
    }
}