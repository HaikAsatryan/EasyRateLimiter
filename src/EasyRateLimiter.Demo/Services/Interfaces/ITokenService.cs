using EasyRateLimiter.Demo.Contexts;
using EasyRateLimiter.Demo.DTOs.Token;
using EasyRateLimiter.Demo.Models;

namespace EasyRateLimiter.Demo.Services.Interfaces;

public interface ITokenService
{
    public Task<Token> CreateTokenAsync(IdentifyUserDto user, HttpContext httpContext);
    public Task<IdentifyTokenDto> ValidateTokenAsync(MemoryDb memoryDb, HttpContext httpContext);
    
    public Task UpdateTokenExpirationAsync(IdentifyTokenDto token, IConfiguration configuration, MemoryDb memoryDb, HttpContext httpContext);
    
}