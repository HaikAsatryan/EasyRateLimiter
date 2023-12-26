using EasyRateLimiter.Demo.Enums;

namespace EasyRateLimiter.Demo.DTOs.User
{
    public class GetUserDto
    {
        public long Id { get; set; }
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public RolesSelect Role { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Statuses Status { get; set; }
        public string? Comment { get; set; } 
    }
}