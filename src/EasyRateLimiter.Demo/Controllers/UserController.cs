using EasyRateLimiter.Demo.Attributes;
using EasyRateLimiter.Demo.DTOs.User;
using EasyRateLimiter.Demo.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EasyRateLimiter.Demo.Controllers;

[ApiController]
[Route("api/v1/user")]
[Produces("application/json")]
[Authorize]
public class UserController(IUserService service) : Controller
{
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
    {
        await service.CreateUserAsync(createUserDto);
        return Ok();
    }


    [HttpPatch]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updateUserDto)
    {
        await service.UpdateUserAsync(updateUserDto);
        return Ok();
    }

    [HttpPatch("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] UpdatePasswordDto updatePasswordDto)
    {
        await service.UpdatePasswordAsync(updatePasswordDto);
        return Ok();
    }

    [HttpPatch("status")]
    public async Task<IActionResult> UpdateUserStatus([FromBody] UpdateUserStatusDto updateUserStatusDto)
    {
        await service.UpdateUserStatusAsync(updateUserStatusDto);
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteUsers(List<long> ids)
    {
        await service.DeleteUsersAsync(ids);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(int page, int pageSize)
    {
        var data = await service.GetUsersAsync(page, pageSize);

        return Ok(data);
    }
}