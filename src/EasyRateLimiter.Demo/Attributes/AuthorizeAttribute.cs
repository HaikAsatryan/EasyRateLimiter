using System.Diagnostics.CodeAnalysis;
using EasyRateLimiter.Demo.Contexts;
using EasyRateLimiter.Demo.DTOs.User;
using EasyRateLimiter.Demo.Enums;
using EasyRateLimiter.Demo.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using ResponseCrafter.StandardHttpExceptions;

namespace EasyRateLimiter.Demo.Attributes;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
public class AuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly Roles _minimalRoleLevel;

    public AuthorizeAttribute()
    {
        _minimalRoleLevel = Roles.Admin;
    }

    public AuthorizeAttribute(Roles role)
    {
        _minimalRoleLevel = role;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var isExcluded = context.ActionDescriptor.EndpointMetadata
            .Any(em => em.GetType() == typeof(Anonymous));

        if (isExcluded)
            return;

        var contextUser = context.HttpContext.RequestServices.GetRequiredService<ContextUser>();
        var dbContext = context.HttpContext.RequestServices.GetRequiredService<MemoryDb>();
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();

        var token = await tokenService.ValidateTokenAsync(dbContext, context.HttpContext);

        var user = token.User;
        if (user.Role > _minimalRoleLevel)
            throw new ForbiddenException();

        await tokenService.UpdateTokenExpirationAsync(token, configuration, dbContext, context.HttpContext);

        userService.SetUserContext(token, contextUser);
    }
}