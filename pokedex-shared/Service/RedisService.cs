﻿using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using pokedex_shared.Common;

namespace pokedex_shared.Service;

public class RedisService(
    ILogger<DatasourceQueryService> logger,
    IDistributedCache redis)
{
    public async Task<T> GetValueTypeAsync<T>(string key,
        CancellationToken cancellationToken = default) where T : struct
    {
        var cacheValue = await redis.GetStringAsync(key, cancellationToken);
        if (cacheValue is null)
        {
            logger.LogInformation("Cache miss - {key}", key);
            return default;
        }

        logger.LogInformation("Cache hit - {key}", key);

        return await cacheValue.DeserializeValueTypeToJsonAsync<T>(cancellationToken);
    }

    public async Task SetAsync(
        string key,
        string json,
        DistributedCacheEntryOptions options,
        CancellationToken cancellationToken = default)
    {
        await redis.SetStringAsync(key, json, options, cancellationToken);
    }
}