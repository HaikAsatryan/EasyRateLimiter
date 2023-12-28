using StackExchange.Redis;

namespace EasyRateLimiter.Repositories;

public class RedisLuaRepository(IConnectionMultiplexer redisConnectionMultiplexer)
{
    private readonly IDatabaseAsync _redisDatabase = redisConnectionMultiplexer.GetDatabase();

     // private const string LuaScript = """
     //                                  local key = KEYS[1]
     //                                  local nowTicks = tonumber(ARGV[1])
     //                                  local maxExpirySeconds = tonumber(ARGV[2])
     //                                  local maxExpiryTicks = tonumber(ARGV[3])
     //                                  
     //                                  -- Add current timestamp to the list
     //                                  redis.call('RPUSH', key, nowTicks)
     //                                  
     //                                  -- Trim the list to remove timestamps older than 'now - maxExpiry'
     //                                  local threshold = nowTicks - maxExpiryTicks
     //                                  local ticks = redis.call('LRANGE', key, 0, -1)
     //                                  local validIndex = 0
     //                                  for i, tick in ipairs(ticks) do
     //                                      if tonumber(tick) > threshold then
     //                                          validIndex = i - 1
     //                                          break
     //                                      end
     //                                  end
     //                                  redis.call('LTRIM', key, validIndex, -1)
     //                                  -- Set expiry for the key
     //                                  redis.call('EXPIRE', key, maxExpirySeconds)
     //                                  
     //                                  -- Return the updated list
     //                                  return redis.call('LRANGE', key, 0, -1)
     //                                  """;

     private const string LuaScript = """
                                      local key = KEYS[1]
                                      local nowTicks = tonumber(ARGV[1])
                                      local maxExpirySeconds = tonumber(ARGV[2])
                                      local maxExpiryTicks = tonumber(ARGV[3])

                                      -- Add current timestamp to the sorted set
                                      redis.call('ZADD', key, nowTicks, nowTicks)

                                      -- Remove timestamps older than 'now - maxExpiry'
                                      local threshold = nowTicks - maxExpiryTicks
                                      redis.call('ZREMRANGEBYSCORE', key, '-inf', threshold)

                                      -- Set expiry for the key
                                      redis.call('EXPIRE', key, maxExpirySeconds)

                                      -- Return the updated set
                                      return redis.call('ZRANGE', key, 0, -1)
                                      """;

    public async Task<List<long>> GetAndUpdateCacheEntriesAsync(string key, long nowTicks,
        int expirationSeconds, long expirationTicks)
    {
        var redisKey = new RedisKey[] { key };
        var maxExpiryTicks = nowTicks - expirationTicks;

        var redisValues = new RedisValue[]
        {
            nowTicks.ToString(),
            expirationSeconds.ToString(),
            maxExpiryTicks.ToString()
        };

        var result = await _redisDatabase.ScriptEvaluateAsync(LuaScript, redisKey, redisValues);
        return ParseRedisResult(result);
    }


    private static List<long> ParseRedisResult(RedisResult result)
    {
        var redisValues = (RedisValue[])result!;
        var ticks = new List<long>(redisValues.Length);

        ticks.AddRange(redisValues.Select(redisValue => long.Parse(redisValue.ToString())));

        return ticks;
    }
}