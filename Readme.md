# 1. EasyRateLimiter

- [1. EasyRateLimiter](#1-easyratelimiter)
  - [1.1. Introduction](#11-introduction)
  - [1.2. Installation](#12-installation)
    - [1.2.1. Program.cs Example](#121-programcs-example)
    - [1.2.2. Appsettings.json Example](#122-appsettingsjson-example)
  - [1.3. Configuration Options Explained](#13-configuration-options-explained)
  - [1.4. Middleware Registration](#14-middleware-registration)
  - [1.5. Performance Insights](#15-performance-insights)
  - [1.6. Limitations](#16-limitations)
  - [1.7. License](#17-license)

## 1.1. Introduction
**EasyRateLimiter** is my project which I wanted to share as NuGet package but then realized that native microsoft solution is 4 times faster. Key features include:

- **Dual Functionality:** Offers both basic and distributed (Redis-backed) rate limiting, accommodating a wide range of use cases.
- **Ease of Integration:** Designed with simplicity in mind, it integrates seamlessly into existing projects with minimal configuration.
- **Flexibility:** Compatible with various application scenarios, supporting IP and client ID-based rate limiting.
- **Customization:** Extensive configuration options allow fine-tuning to meet specific application requirements.
  This package is ideal for developers seeking an easy-to-use yet powerful solution for controlling traffic and preventing overuse of resources in .NET-based applications.

## 1.2. Installation

To incorporate EasyRateLimiter into your project, you can choose between two primary methods of setup in your `program.cs`:

- `builder.AddRateLimiter();`
- `builder.AddDistributedRateLimiter();`

Configuration options can be specified either in `appsettings.json` or directly in `program.cs`, with the latter taking precedence.

### 1.2.1. Program.cs Example

```csharp
builder.AddRateLimiter(options =>
{
    options.ClientWhitelist = ["12.94.29.19", "127.0.0.1"];
    options.EndpointWhitelist = ["/api/WeatherForecast"];
    options.EnableIpRateLimiting = true;
});

```

### 1.2.2. Appsettings.json Example

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },

  "RateLimiter": {
    "EnableIpRateLimiting": true,
    "EnableClientIdRateLimiting": true,
    "IpHeader": "",
    "ClientIdHeader": "ClientId",
    "HttpStatusCode": 429,
    "IpWhitelist": ["127.0.1.1", "::11"],
    "EndpointWhitelist": ["put:/api/v1/rates", "*:/above-board/pings"],
    "ClientWhitelist": ["1", "34"],
    "Rules": [
      {
        "Endpoint": "*",
        "Period": "30m",
        "Limit": 1000
      },
      {
        "Endpoint": "get:/above-board/ping",
        "Period": "30m",
        "Limit": 400
      },
      {
        "Endpoint": "*:/above-board/headers",
        "Period": "15m",
        "Limit": 100
      },
      {
        "Endpoint": "post:/api/v1/login",
        "Period": "1h",
        "Limit": 1000
      },
      {
        "Endpoint": "*:/api/v1/hello",
        "Period": "1d",
        "Limit": 10000
      }
    ]
  }
}
```

The configuration options in `appsettings.json` and `program.cs` (priority) are identical.

## 1.3. Configuration Options Explained

- **Enable Options:** These are simple toggles to quickly enable or disable rate limiting.
- **IpHeader:** Specifies the custom header key for IP addresses in HTTP requests. If left empty, the package attempts to resolve the IP from `x-forwarded-for`, then `Forwarded`, and finally `remoteAddress`.
- **ClientIdHeader:** Used for specifying the client/user ID key in HTTP headers.
- **HttpStatus Code:** Defaults to 429 (Too Many Requests) if not set.
- **Whitelists:** Straightforward lists where you specify IPs, clients, or endpoints exempt from rate limiting.
- **Rules:** Define the rate limiting rules. Notably, if a generic rule (e.g., \*) and a specific rule (e.g., get:/login) both apply to an endpoint, the endpoint adheres to the stricter of the two.

## 1.4. Middleware Registration

Two middleware options are available:

- **app.UseIpRateLimiter();** (Should be placed above authentication)
- **app.UseClientRateLimiter();** (Should be placed below authentication)
  The order of middleware registration is important.

For distributed rate limiting, the package defaults to finding the Redis connection string in `appsettings.json` under `{"ConnectionStrings": {"Redis":}}`. This can be overridden in `program.cs`:

```csharp
var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
builder.AddDistributedRateLimiter(redisConnectionString!);
```

## 1.5. Performance Insights

Our performance benchmarks provide a clear understanding of what to expect in terms of efficiency and resource utilization:

- **Standard Rate Limiter (`builder.AddRateLimiter();`):** In scenarios with over 30 endpoints, each receiving 1 request per second, the average processing time per request is approximately 4.5 milliseconds (Can reach up to 9 ms on DDOS attacks). This is almost 7 times slower than System.Threading.RateLimiter.

- **Distributed Rate Limiter (`builder.AddDistributedRateLimiter();`):** Under similar conditions, the average time for processing each request is about 10 milliseconds and can reach up to 80ms on DDOS attacks. This significantly higher response time is due to the nature of distributed systems and network latency involved in communicating with Redis. There are plenty of better solutions that's why this is just personal working.

- **Memory Footprint:** Both standard and distributed versions maintain a minimal memory footprint. The estimated memory usage is roughly 20 bytes per request, translating to a maximum of around 1MB RAM for 30+ endpoints under continuous use. This efficient memory usage is vital for applications where resource optimization is critical.

- **Comparative Analysis:** Compared to other rate limiting solutions, EasyRateLimiter stands out for its simplicity. While it may not employ complex algorithms like token bucket or concurrency, it has solid implementation of sliding window algorithm. As it is custom implementation its not as performant as Microsoft one.


## 1.6. Limitations

EasyRateLimiter does not implement advanced algorithms like token bucket or concurrency limiter. It currently supports .NET 8+ environments only.
**Though this repo is super stable its not intended for production use** `RedisRateLimiting.AspNetCore` and `System.Threading.RateLimiting` are much better solutions in terms of performance. They are worse in terms of configuration and ease of use but it's totally subjective and depends on your use case.

## 1.7. License

EasyRateLimiter is licensed under the MIT License.