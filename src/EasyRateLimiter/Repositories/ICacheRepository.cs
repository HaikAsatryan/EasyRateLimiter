namespace EasyRateLimiter.Repositories;

public interface ICacheRepository
{
    public Task CreateCacheEntryAsync(string key, List<DateTime> value, TimeSpan cacheTimeSpan);
    public Task<List<DateTime>?> GetCacheEntriesAsync(string key);
    
    public Task UpdateCacheEntryAsync(string key, List<DateTime> value, TimeSpan cacheTimeSpan);
}