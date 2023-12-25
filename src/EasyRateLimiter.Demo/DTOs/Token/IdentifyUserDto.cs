using System.Diagnostics.CodeAnalysis;
using EasyRateLimiter.Demo.Enums;

namespace EasyRateLimiter.Demo.DTOs.Token;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class IdentifyUserDto
{
    public long Id { get; set; }

    public string Username { get; set; } = null!;
    public Roles Role { get; set; }
}