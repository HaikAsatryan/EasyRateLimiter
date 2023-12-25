using System.ComponentModel.DataAnnotations;

namespace EasyRateLimiter.Demo.DTOs.Authentication;

public class LoginDto
{
    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}