using EasyRateLimiter.Demo.Contexts;
using EasyRateLimiter.Demo.Enums;
using EasyRateLimiter.Demo.Models;

namespace EasyRateLimiter.Demo.Extensions;

public static class DatabaseExtension
{
    public static WebApplicationBuilder AddInMemoryDb(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<MemoryDb>();
        return builder;
    }

    public static WebApplication MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MemoryDb>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        var user = new List<User>
        {
            new()
            {
                Id = 1,
                Username = "admin",
                FullName = "Admin",
                Password = "admin",
                Role = Roles.Admin,
                CreatedAt = DateTime.UtcNow,
                Status = Statuses.Active
            },
            new()
            {
                Id = 2,
                Username = "user",
                FullName = "User",
                Password = "user",
                Role = Roles.User,
                CreatedAt = DateTime.UtcNow,
                Status = Statuses.Active
            }
        };
        dbContext.Users.AddRange(user);
        dbContext.SaveChanges();

        return app;
    }
}