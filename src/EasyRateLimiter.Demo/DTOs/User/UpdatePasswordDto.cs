using System.Text.Json.Serialization;

namespace EasyRateLimiter.Demo.DTOs.User
{
    public class UpdatePasswordDto
    {
        public long Id { get; set; }
        public string NewPassword { get; set; } = null!;
    }
}