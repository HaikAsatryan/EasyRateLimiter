using EasyRateLimiter.Demo.Extensions;
using EasyRateLimiter.Extensions;
using ResponseCrafter;

var builder = WebApplication.CreateBuilder(args);
builder.AddInMemoryDb()
    .RegisterAllServices()
    .AddPandaSwagger()
    .AddResponseCrafter();


//builder.AddRateLimiter();

builder.AddDistributedRateLimiter(); //from nuget package

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
var app = builder.Build();

app.UseIpRateLimiter(); //from nuget package

app.UseStaticFiles();

app.UseResponseCrafter()
    .MigrateDatabase()
    .UsePandaSwagger();

//Adding custom endpoints
app.MapPandaStandardEndpoints();

app.MapControllers();
app.Run();