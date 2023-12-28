using Microsoft.Extensions.Caching.Memory;

namespace EasyRateLimiter.Repositories;

public class MemoryCacheRepository(IMemoryCache cache)
{
    public void CreateCacheEntryAsync(string key, List<long> value, TimeSpan expiration)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(expiration);

        cache.Set(key, value, cacheEntryOptions);
    }

    public List<long>? GetCacheEntriesAsync(string key)
    {
        var trySuccess = cache.TryGetValue(key, out List<long>? value);
        return trySuccess ? value : null;
    }

    public void UpdateCacheEntryAsync(string key, List<long> value, TimeSpan cacheTimeSpan)
    {
        CreateCacheEntryAsync(key, value, cacheTimeSpan);
    }

    public void DeleteCacheEntryAsync(string key)
    {
        cache.Remove(key);
    }
}