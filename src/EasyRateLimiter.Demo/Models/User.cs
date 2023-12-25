using EasyRateLimiter.Demo.Enums;
using Microsoft.EntityFrameworkCore;

namespace EasyRateLimiter.Demo.Models;

[PrimaryKey(nameof(Id))]
[Index(nameof(Username), IsUnique = true)]
[Index(nameof(FullName))]
public class User
{
    public long Id { get; set; }
    public string Username { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public Roles Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public Statuses Status { get; set; }
    public string? Comment { get; set; }
}