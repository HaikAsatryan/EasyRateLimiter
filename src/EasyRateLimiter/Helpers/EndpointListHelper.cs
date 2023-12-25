namespace EasyRateLimiter.Helpers;

public static class EndpointListHelper
{
    public static List<string>? Build(List<string>? endpoints)
    {
        if (endpoints == null)
        {
            return null;
        }

        var allHttpMethods = new List<string> { "get", "post", "put", "delete", "patch", "head", "options", "trace" };

        var parsedEndpoints = new List<string>();

        foreach (var endpoint in endpoints)
        {
            if (endpoint.StartsWith("*"))
            {
                parsedEndpoints.AddRange(allHttpMethods.Select(method => method + endpoint[1..].ToLower()));
            }
            else
            {
                parsedEndpoints.Add(endpoint.ToLower());
            }
        }

        return parsedEndpoints;
    }

    public static List<string> Build(string endpoint)
    {
        var allHttpMethods = new List<string> { "get", "post", "put", "delete", "patch", "head", "options", "trace" };

        var parsedEndpoints = new List<string>();


        if (endpoint.StartsWith("*"))
        {
            parsedEndpoints.AddRange(allHttpMethods.Select(httpMethod => httpMethod + endpoint[1..].ToLower()));
        }
        else
        {
            parsedEndpoints.Add(endpoint.ToLower());
        }


        return parsedEndpoints;
    }
}