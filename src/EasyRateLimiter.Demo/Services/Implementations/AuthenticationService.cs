using System.Diagnostics.CodeAnalysis;
using EasyRateLimiter.Demo.Contexts;
using EasyRateLimiter.Demo.DTOs.Authentication;
using EasyRateLimiter.Demo.DTOs.Token;
using EasyRateLimiter.Demo.DTOs.User;
using EasyRateLimiter.Demo.Enums;
using EasyRateLimiter.Demo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ResponseCrafter.StandardHttpExceptions;

namespace EasyRateLimiter.Demo.Services.Implementations;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
public class AuthenticationService : IAuthenticationService
{
    private readonly MemoryDb _context;
    private readonly ContextUser _contextUser;
    private readonly ITokenService _tokenService;
    private readonly HttpContext _httpContext;

    public AuthenticationService(MemoryDb context,
        ContextUser contextUser, ITokenService tokenService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _contextUser = contextUser;
        _tokenService = tokenService;
        _httpContext = httpContextAccessor.HttpContext!;
    }

    public async Task LoginAsync(LoginDto loginDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username.ToLower());
        if (user == null || user.Status == Statuses.Deleted)
        {
            throw new BadRequestException("invalid_username_or_password");
        }

        if (user.Status == Statuses.Disabled)
        {
            throw new BadRequestException("invalid_username_or_password");
        }


        if (loginDto.Password != user.Password)
        {
            throw new BadRequestException("invalid_username_or_password");
        }
        
        var identifiedUser = new IdentifyUserDto
        {
            Id = user.Id,
            Role = user.Role,
            Username = user.Username
        };
        await _tokenService.CreateTokenAsync(identifiedUser, _httpContext);
    }

    public async Task LogoutAsync()
    {
        var tokenId = _contextUser.TokenId;
        var token = await _context.Tokens.FirstOrDefaultAsync(t => t.Id == tokenId);
        token!.ExpirationDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        foreach (var cookie in _httpContext.Request.Cookies)
        {
            _httpContext.Response.Cookies.Delete(cookie.Key);
        }
    }

    public async Task UpdateOwnPassword(UpdateOwnPasswordDto updateOwnPasswordDto)
    {
       var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == _contextUser.TokenId);

        if (user == null)
            throw new NotFoundException();

        if (user.Role == Roles.SuperAdmin)
            throw new ForbiddenException("superadmin_password_cannot_be_changed");

        if (updateOwnPasswordDto.OldPassword != user.Password)
            throw new BadRequestException("wrong_old_password");

        user.Password = updateOwnPasswordDto.NewPassword;
        await _context.SaveChangesAsync();

        await LogoutAllExceptCurrentSessionAsync(user.Id, _contextUser.Id);
    }

    public async Task LogoutAllAsync(long userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
            throw new NotFoundException();

        var tokens = await _context.Tokens.Where(t => t.UserId == userId).ToListAsync();

        if (tokens.Count == 0)
            return;

        foreach (var userToken in tokens.Where(t => t.ExpirationDate > DateTime.UtcNow))
        {
            userToken.ExpirationDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    private async Task LogoutAllExceptCurrentSessionAsync(long userId, long tokenId)
    {
        var tokens = await _context.Tokens
            .Where(t => t.UserId == userId && t.Id != tokenId).ToListAsync();

        foreach (var expiredUserToken in tokens.Where(t => t.ExpirationDate > DateTime.UtcNow))
        {
            expiredUserToken.ExpirationDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}