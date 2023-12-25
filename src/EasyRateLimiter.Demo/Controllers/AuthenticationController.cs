using EasyRateLimiter.Demo.Attributes;
using EasyRateLimiter.Demo.DTOs.Authentication;
using EasyRateLimiter.Demo.Enums;
using EasyRateLimiter.Demo.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EasyRateLimiter.Demo.Controllers;

[ApiController]
[Route("api/v1/authentication")]
[Produces("application/json")]
[Authorize(Roles.User)]
public class AuthenticationController(IAuthenticationService service) : Controller
{
    [Anonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
    {
        await service.LoginAsync(loginDto);

        return Ok();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        await service.LogoutAsync();
        return Ok();
    }

    [HttpPatch("change-own-password")]
    public async Task<IActionResult> ChangeOwnPassword([FromBody] UpdateOwnPasswordDto updatePasswordDto)
    {
        await service.UpdateOwnPassword(updatePasswordDto);
        return Ok();
    }
}