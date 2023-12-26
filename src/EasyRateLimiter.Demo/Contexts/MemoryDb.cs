using EasyRateLimiter.Demo.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyRateLimiter.Demo.Contexts;

public class MemoryDb:DbContext
{
    protected override void OnConfiguring
        (DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase(databaseName: "TestDb");
    }
    public DbSet<Token> Tokens { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    
}