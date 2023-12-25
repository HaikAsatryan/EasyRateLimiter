using System.ComponentModel.DataAnnotations;

namespace EasyRateLimiter.Demo.DTOs.Authentication
{
    public class UpdateOwnPasswordDto
    {
        [Required]
        public string OldPassword { get; set; } = null!;
        [Required]
        public string NewPassword { get; set; } = null!;
    }
}