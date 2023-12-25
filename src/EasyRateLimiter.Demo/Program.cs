using EasyRateLimiter.Demo.Extensions;
using EasyRateLimiter.Extensions;
using ResponseCrafter;

var builder = WebApplication.CreateBuilder(args);
builder.AddInMemoryDb()
    .RegisterAllServices()
    .AddPandaSwagger()
    .AddResponseCrafter();


builder.AddRateLimiter();
var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
builder.AddDistributedRateLimiter();
builder.AddDistributedRateLimiter(redisConnectionString!); //from nuget package

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
var app = builder.Build();

app.UseIpRateLimiter(); //from nuget package
app.UseClientRateLimiter();
app.UseStaticFiles();
app.UseResponseCrafter()
    .MigrateDatabase()
    .UsePandaSwagger();

//Adding custom endpoints
app.MapPandaStandardEndpoints();

app.MapControllers();
app.Run();