// =============================================================================
// Service: MemoryRagCacheService
// Chuc nang: In-memory cache implementation (Phase 5). Single instance only -
//            khi scale ra nhieu API replica, can swap qua Redis.
// =============================================================================
using Microsoft.Extensions.Caching.Memory;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.Infrastructure.Services;

public class MemoryRagCacheService : IRagCacheService
{
    private readonly IMemoryCache _cache;

    public MemoryRagCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        _cache.Set(key, value, ttl);
        return Task.CompletedTask;
    }
}
