using Microsoft.Extensions.Caching.Memory;

namespace EasyRateLimiter.Repositories;

public class MemoryCacheRepository(IMemoryCache cache) : ICacheRepository
{
    public Task CreateCacheEntryAsync(string key, List<DateTime> value, TimeSpan expiration)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(expiration);

        cache.Set(key, value, cacheEntryOptions);
        return Task.CompletedTask;
    }

    public Task<List<DateTime>?> GetCacheEntriesAsync(string key)
    {
        var trySuccess = cache.TryGetValue(key, out List<DateTime>? value);
        return Task.FromResult(trySuccess ? value : null);
    }

    public Task UpdateCacheEntryAsync(string key, List<DateTime> value, TimeSpan cacheTimeSpan)
    {
        CreateCacheEntryAsync(key, value, cacheTimeSpan);
        return Task.CompletedTask;
    }

    public Task DeleteCacheEntryAsync(string key)
    {
        cache.Remove(key);
        return Task.CompletedTask;
    }
}