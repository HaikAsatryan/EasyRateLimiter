using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace EasyRateLimiter.Repositories;

public class RedisCacheRepository(
    IConnectionMultiplexer redisConnectionMultiplexer,
    ILogger<RedisCacheRepository> logger) : ICacheRepository
{
    private readonly IDatabaseAsync _redisDatabase = redisConnectionMultiplexer.GetDatabase();
    private readonly RedisValue _token = Environment.MachineName;

    public async Task CreateCacheEntryAsync(string key, List<DateTime> values, TimeSpan expiration)
    {
        var lockKey = $"{key}:lock";
        try
        {
            var serializedValue = JsonConvert.SerializeObject(values);
            await _redisDatabase.StringSetAsync(key, serializedValue, expiration);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create cache entry. {ExceptionMessage}", ex.Message);
            throw new Exception("Failed to create cache entry", ex);
        }
        finally
        {
            await _redisDatabase.LockReleaseAsync(lockKey, _token);
        }
    }

    public async Task<List<DateTime>?> GetCacheEntriesAsync(string key)
    {
        var lockKey = $"{key}:lock";
        var maxLockTime = TimeSpan.FromMilliseconds(20);

        var isLockAcquired = await TryAcquireLockWithRetriesAsync(lockKey, maxLockTime);
        if (isLockAcquired)
        {
            try
            {
                var serializedValue = await _redisDatabase.StringGetAsync(key);
                if (!serializedValue.IsNullOrEmpty)
                {
                    return JsonConvert.DeserializeObject<List<DateTime>>(serializedValue!);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve cache entry. {ExceptionMessage}", ex.Message);
                throw new Exception("Failed to retrieve cache entry", ex);
            }
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

    private async Task<bool> TryAcquireLockWithRetriesAsync(string lockKey, TimeSpan maxLockTime)
    {
        const int maxRetries = 10;

        var attempt = 0;
        var retryDelay = TimeSpan.FromMilliseconds(1);

        while (attempt < maxRetries)
        {
            var isLockAcquired = await _redisDatabase.LockTakeAsync(lockKey, _token, maxLockTime);
            if (isLockAcquired)
            {
                return true;
            }

            await Task.Delay(retryDelay);
            attempt++;
            retryDelay = TimeSpan.FromTicks(retryDelay.Ticks * 2);
        }

        return false; // Failed to acquire the lock after retries
    }
}