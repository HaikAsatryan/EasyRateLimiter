using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace EasyRateLimiter.Repositories;

public class RedisCacheRepositoryNoLock(IConnectionMultiplexer redisConnectionMultiplexer) : ICacheRepository
{
    private readonly IDatabaseAsync _redisDatabase = redisConnectionMultiplexer.GetDatabase();

    public async Task CreateCacheEntryAsync(string key, List<DateTime> values, TimeSpan expiration)
    {
        var serializedValue = JsonConvert.SerializeObject(values);
        await _redisDatabase.StringSetAsync(key, serializedValue, expiration);
    }

    public async Task<List<DateTime>?> GetCacheEntriesAsync(string key)
    {
        var serializedValue = await _redisDatabase.StringGetAsync(key);
        if (!serializedValue.IsNullOrEmpty)
        {
            return JsonConvert.DeserializeObject<List<DateTime>>(serializedValue!);
        }

        return null;
    }

    public async Task UpdateCacheEntryAsync(string key, List<DateTime> value, TimeSpan cacheTimeSpan)
    {
        await CreateCacheEntryAsync(key, value, cacheTimeSpan);
    }

    public Task DeleteCacheEntryAsync(string key)
    {
        return _redisDatabase.KeyDeleteAsync(key);
    }
}