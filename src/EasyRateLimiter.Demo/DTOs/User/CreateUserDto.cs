using System.ComponentModel.DataAnnotations;
using EasyRateLimiter.Demo.Enums;

namespace EasyRateLimiter.Demo.DTOs.User
{
    public class CreateUserDto
    {
        [Required]
        public string Username { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!; 
        [Required]
        public string FullName { get; set; } = null!;
        [Required]
        public Roles Role { get; set; }
        public string? Comment { get; set; }
    }
}