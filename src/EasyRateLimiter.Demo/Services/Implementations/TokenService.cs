using System.Diagnostics.CodeAnalysis;
using EasyRateLimiter.Demo.Contexts;
using EasyRateLimiter.Demo.DTOs.Token;
using EasyRateLimiter.Demo.Enums;
using EasyRateLimiter.Demo.Models;
using EasyRateLimiter.Demo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ResponseCrafter.StandardHttpExceptions;

namespace EasyRateLimiter.Demo.Services.Implementations;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
public class TokenService : ITokenService
{
    private readonly string _tokenExpirationMinutes;
    private readonly string _tokenMaxExpirationMinutes;
    private readonly string _domain;
    private readonly MemoryDb _context;

    public TokenService(IConfiguration configuration, MemoryDb context)
    {
        _context = context;
        _tokenExpirationMinutes = configuration["Security:TokenExpirationMinutes"]!;
        _tokenMaxExpirationMinutes = configuration["Security:TokenMaxExpirationMinutes"]!;
        _domain = configuration["Security:CookieDomain"]!;
    }

    public async Task<Token> CreateTokenAsync(IdentifyUserDto user, HttpContext httpContext)
    {
        var expirationMinutesInt = 15;

        if (int.TryParse(_tokenExpirationMinutes, out var expirationMinutes))
        {
            expirationMinutesInt = expirationMinutes;
        }

        var maxExpirationMinutesInt = 360;

        if (int.TryParse(_tokenMaxExpirationMinutes, out var maxExpirationMinutes))
        {
            maxExpirationMinutesInt = maxExpirationMinutes;
        }

        if (expirationMinutesInt > maxExpirationMinutesInt)
        {
            expirationMinutesInt = maxExpirationMinutesInt;
        }

        var tokenSignature = Guid.NewGuid();

        var token = new Token
        {
            Signature = tokenSignature,
            ExpirationDate = DateTime.UtcNow.AddMinutes(expirationMinutesInt),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Tokens.AddAsync(token);
        await _context.SaveChangesAsync();

        var cookies = new Dictionary<string, string>
        {
            { "Token", tokenSignature.ToString() },
            { "UserId", user.Id.ToString() },
            { "Role", ((int)user.Role).ToString() },
            { "Username", user.Username }
        };

        AppendCookies(cookies, httpContext, _domain, token.ExpirationDate);
        return token;
    }

    public async Task<IdentifyTokenDto> ValidateTokenAsync(MemoryDb memoryDb, HttpContext httpContext)
    {
        var tokenSignature = httpContext.Request.Cookies["Token"];

        if (string.IsNullOrEmpty(tokenSignature))
            throw new UnauthorizedException();

        var signature = Guid.Parse(tokenSignature);
        
        var token = await memoryDb.Tokens.Include(z => z.User)
            .FirstOrDefaultAsync(x => x.Signature == signature);

        if (token == null || token.ExpirationDate < DateTime.UtcNow || token.User.Status != Statuses.Active)
            throw new UnauthorizedException();

        var response = new IdentifyTokenDto
        {
            TokenId = token.Id,
            TokenSignature = signature,
            CreatedAt = token.CreatedAt,
            ExpirationDate = token.ExpirationDate,
            User = new IdentifyUserDto
            {
                Id = token.User.Id,
                Role = token.User.Role,
                Username = token.User.Username
            }
        };

        return response;
    }

    public async Task UpdateTokenExpirationAsync(IdentifyTokenDto token, IConfiguration configuration,
        MemoryDb memoryDb,
        HttpContext httpContext)
    {
        var expirationMinutesInt = 15;
        if (int.TryParse(configuration["Security:TokenExpirationMinutes"], out var expirationMinutes))
        {
            expirationMinutesInt = expirationMinutes;
        }

        var maxExpirationMinutesInt = 360;
        if (int.TryParse(configuration["Security:TokenMaxExpirationMinutes"], out var maxExpirationMinutes))
        {
            maxExpirationMinutesInt = maxExpirationMinutes;
        }

        var newExpirationDate = DateTime.UtcNow.AddMinutes(expirationMinutesInt);

        if (newExpirationDate < token.CreatedAt.AddMinutes(maxExpirationMinutesInt))
        {
            token.ExpirationDate = newExpirationDate;
        }
        else
        {
            token.ExpirationDate = token.CreatedAt.AddMinutes(maxExpirationMinutesInt);
        }

        await memoryDb.SaveChangesAsync();
        var cookies = new Dictionary<string, string>
        {
            { "Token", token.TokenSignature.ToString() },
            { "UserId", token.User.Id.ToString() },
            { "Role", ((int)token.User.Role).ToString() },
            { "Username", token.User.Username }
        };
        AppendCookies(cookies, httpContext, _domain, token.ExpirationDate);
    }

    private static void AppendCookies(Dictionary<string, string> cookies, HttpContext httpContext, string domain,
        DateTime expirationDate)
    {
        foreach (var cookie in cookies)
        {
            httpContext.Response.Cookies.Append(
                cookie.Key, cookie.Value,
                new CookieOptions
                {
                    Expires = expirationDate,
                    HttpOnly = true,
                    Secure = true,
                    Domain = domain
                }
            );
        }
    }
}