using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using EasyRateLimiter.Demo.Enums;

namespace EasyRateLimiter.Demo.DTOs.User
{
    public class UpdateUserStatusDto
    {
        [Required]
        public long Id { get; set; }

        [Required] public Statuses Status { get; set; }
    }
}