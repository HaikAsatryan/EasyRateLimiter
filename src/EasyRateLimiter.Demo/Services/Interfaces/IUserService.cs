using EasyRateLimiter.Demo.DTOs.Token;
using EasyRateLimiter.Demo.DTOs.User;
using ResponseCrafter.Dtos;

namespace EasyRateLimiter.Demo.Services.Interfaces;

public interface IUserService
{
    public Task CreateUserAsync(CreateUserDto createUserDto);
    public Task UpdateUserAsync(UpdateUserDto updateUserDto);
    public Task UpdatePasswordAsync(UpdatePasswordDto updatePasswordDto);
    public Task UpdateUserStatusAsync(UpdateUserStatusDto updateUserStatusDto);
    public Task DeleteUsersAsync(List<long> ids);
    public Task<PagedResponse<GetUserDto>> GetUsersAsync(int page, int pageSize);
    
    public void SetUserContext(IdentifyTokenDto token, ContextUser contextUser);
}