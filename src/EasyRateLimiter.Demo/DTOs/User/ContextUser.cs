using EasyRateLimiter.Demo.Enums;

namespace EasyRateLimiter.Demo.DTOs.User;

public class ContextUser
{
    public long Id { get; set; }
    public string Username { get; set; } = null!;
    public Roles Role { get; set; } 
    public long TokenId { get; set; }
    public DateTime TokenExpirationDate { get; set; }
}