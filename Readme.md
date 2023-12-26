# 1. Pandatech.EasyRateLimiter

- [1. Pandatech.EasyRateLimiter](#1-pandatecheasyratelimiter)
  - [1.1. Introduction- 1. Pandatech.EasyRateLimiter](#11-introduction--1-pandatecheasyratelimiter)
  - [1.2. Installation](#12-installation)
    - [1.2.1. Program.cs Example](#121-programcs-example)
    - [1.2.2. Appsettings.json Example](#122-appsettingsjson-example)
  - [1.3. Configuration Options Explained](#13-configuration-options-explained)
  - [1.4. Middleware Registration](#14-middleware-registration)
  - [1.5. Performance Insights](#15-performance-insights)
  - [1.6. Limitations](#16-limitations)
  - [1.7. License](#17-license)

## 1.1. Introduction
**Pandatech.EasyRateLimiter** is a lightweight, efficient, and versatile NuGet package tailored for implementing rate limiting in .NET applications. Key features include:

- **Dual Functionality:** Offers both basic and distributed (Redis-backed) rate limiting, accommodating a wide range of use cases.
- **Ease of Integration:** Designed with simplicity in mind, it integrates seamlessly into existing projects with minimal configuration.
- **Flexibility:** Compatible with various application scenarios, supporting IP and client ID-based rate limiting.
- **Customization:** Extensive configuration options allow fine-tuning to meet specific application requirements.
  This package is ideal for developers seeking an easy-to-use yet powerful solution for controlling traffic and preventing overuse of resources in .NET-based applications.

## 1.2. Installation

To incorporate Pandatech.EasyRateLimiter into your project, you can choose between two primary methods of setup in your `program.cs`:

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

- **Standard Rate Limiter (`builder.AddRateLimiter();`):** In scenarios with over 30 endpoints, each receiving 1 request per second, the average processing time per request is approximately 4.5 milliseconds. This indicates a high level of efficiency, especially in applications with moderate to high traffic.

- **Distributed Rate Limiter (`builder.AddDistributedRateLimiter();`):** Under similar conditions, the average time for processing each request is about 10 milliseconds. This slightly higher response time is due to the nature of distributed systems and network latency involved in communicating with Redis.

- **Memory Footprint:** Both standard and distributed versions maintain a minimal memory footprint. The estimated memory usage is roughly 20 bytes per cache record, translating to a maximum of around 1MB RAM for 30+ endpoints under continuous use. This efficient memory usage is vital for applications where resource optimization is critical.

- **Comparative Analysis:** Compared to other rate limiting solutions, Pandatech.EasyRateLimiter stands out for its simplicity and low overhead. While it may not employ complex algorithms like token bucket or sliding windows, its performance is well-suited for a wide range of applications, especially where ease of use and integration are priorities.

In summary, Pandatech.EasyRateLimiter provides an optimal balance between performance and ease of use, making it an excellent choice for developers looking to implement rate limiting in their .NET applications without incurring significant resource overhead.

## 1.6. Limitations

Pandatech.EasyRateLimiter does not implement advanced algorithms like token bucket or sliding windows. It currently supports .NET 8+ environments only.
**DISTRIBUTED RATE LIMITING IS IN BETA AND NOT READY FOR PRODUCTION.** It works stable till 20 requests per second but higher than that there are thread locking issues and some requests are passing through validation.**

## 1.7. License

Pandatech.EasyRateLimiter is licensed under the MIT License.

[![GitHub stars](https://img.shields.io/github/stars/pandatech/Public-API-Documentations.svg?style=social&label=Star&maxAge=2592000)](https://GitHub.com/pandatech/Public-API-Documentations/stargazers/)

[![GitHub forks](https://img.shields.io/github/forks/pandatech/Public-API-Documentations.svg?style=social&label=Fork&maxAge=2592000)](https://GitHub.com/pandatech/Public-API-Documentations/network/)

[![GitHub issues](https://img.shields.io/github/issues/pandatech/Public-API-Documentations.svg)](https://GitHub.com/pandatech/Public-API-Documentations/issues/)