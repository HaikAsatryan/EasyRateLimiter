using EasyRateLimiter;
using EasyRateLimiter.Demo.Extensions;
using EasyRateLimiter.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using RedisRateLimiting.AspNetCore;
using ResponseCrafter;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.AddInMemoryDb()
    .RegisterAllServices()
    .AddResponseCrafter();

builder.Services.AddSwaggerGen();

//builder.AddRateLimiter();

builder.AddDistributedRateLimiter(); //from nuget package

 builder.Services.AddRateLimiter(opt => opt
     .AddRedisSlidingWindowLimiter(policyName: "SlidingWindowLimiter", options =>
     {
         options.Window = TimeSpan.FromSeconds(30);
         options.PermitLimit = 1000;
         options.ConnectionMultiplexerFactory = () => ConnectionMultiplexer.Connect("localhost:6379");
     }));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
var app = builder.Build();
app.UseRateLimiter();

//app.UseIpRateLimiter(); //from nuget package

app.UseStaticFiles();
app.UseResponseCrafter()
    .MigrateDatabase();
app.UseSwagger();
app.UseSwaggerUI();


app.MapCustomEndpoints();
app.MapControllers();
app.Run();