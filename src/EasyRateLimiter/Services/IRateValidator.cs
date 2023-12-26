namespace EasyRateLimiter.Services;

public interface IRateValidator
{
    public Task<bool> CheckRateLimitAsync(string ipOrClientId, string endpoint, DateTime now,
        bool checkGlobalLimiting);
}