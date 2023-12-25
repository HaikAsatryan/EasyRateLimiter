using Microsoft.EntityFrameworkCore;

namespace EasyRateLimiter.Demo.Models;

[PrimaryKey(nameof(Id))]
[Index(nameof(Signature))]
[Index(nameof(ExpirationDate))]
public class Token
{
    public long Id { get; set; }
    public Guid Signature { get; set; }

    public User User { get; set; } = null!;
    public long UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpirationDate { get; set; }
}