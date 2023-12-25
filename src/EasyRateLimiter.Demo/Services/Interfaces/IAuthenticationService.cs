using EasyRateLimiter.Demo.DTOs.Authentication;

namespace EasyRateLimiter.Demo.Services.Interfaces;

public interface IAuthenticationService
{
    public Task LoginAsync(LoginDto loginDto);
    public Task LogoutAsync();
    public Task UpdateOwnPassword(UpdateOwnPasswordDto updateOwnPasswordDto);
    public Task LogoutAllAsync(long userId);
}