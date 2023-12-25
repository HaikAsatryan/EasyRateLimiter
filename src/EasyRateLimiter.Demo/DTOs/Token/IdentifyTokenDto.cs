using System.Diagnostics.CodeAnalysis;

namespace EasyRateLimiter.Demo.DTOs.Token;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class IdentifyTokenDto
{
    public long TokenId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpirationDate { get; set; }
    public Guid TokenSignature { get; set; }
    public IdentifyUserDto User { get; set; } = null!;
}