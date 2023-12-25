using EasyRateLimiter.Demo.DTOs.User;
using EasyRateLimiter.Demo.Services.Implementations;
using EasyRateLimiter.Demo.Services.Interfaces;

namespace EasyRateLimiter.Demo.Extensions;

public static class RegisterServicesExtension
{
    public static WebApplicationBuilder RegisterAllServices(this WebApplicationBuilder builder)
    {
        builder.AddServices();


        builder.Services.AddHttpContextAccessor();
        return builder;
    }


    private static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
        builder.Services.AddScoped<ContextUser>();
        builder.Services.AddScoped<ITokenService, TokenService>();

        return builder;
    }
}