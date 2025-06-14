using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace JobSeeker.Deduplication.Caching.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="IDistributedCache" /> interface to manage cached data with enhanced functionality
/// </summary>
public static class DistributedCacheExtension
{
    /// <summary>
    ///     Asynchronously sets a record in the distributed cache with the specified key and value,
    ///     along with optional expiration settings
    /// </summary>
    /// <typeparam name="T">The type of the value to be stored in the cache</typeparam>
    /// <param name="cache">The distributed cache instance</param>
    /// <param name="key">The unique key to identify the cached record</param>
    /// <param name="value">The value to be stored in the cache</param>
    /// <param name="absoluteExpiration">The optional time span for the record's absolute expiration (10 minutes by default)</param>
    /// <param name="unusedExpiration">The optional time span for the sliding expiration of the record (5 minutes by default)</param>
    public static async Task SetRecordAsync<T>(this IDistributedCache cache,
        string key, T value,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? unusedExpiration = null)
    {
        var options = new DistributedCacheEntryOptions();

        options.AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromMinutes(10);
        options.SlidingExpiration = unusedExpiration;

        var jsonData = JsonSerializer.Serialize(value);

        await cache.SetStringAsync(key, jsonData, options);
    }

    /// <summary>
    ///     Asynchronously retrieves a cached record from the distributed cache using the specified key
    /// </summary>
    /// <typeparam name="T">The type of the value to be stored in the cache</typeparam>
    /// <param name="cache">The distributed cache instance</param>
    /// <param name="key">The unique key to identify the cached record</param>
    /// <returns>The deserialized value of the specified type stored in the cache, or the default value of the type if the key does not exist</returns>
    public static async Task<T?> GetRecordAsync<T>(this IDistributedCache cache, string key)
    {
        var jsonData = await cache.GetStringAsync(key);
        return jsonData == null ? default : JsonSerializer.Deserialize<T>(jsonData);
    }
}