// =============================================================================
// Interface: IRagCacheService
// Chuc nang: Cache key-value cho ket qua RAG (retrieval search, ...) - Phase 5.
//            Wrap quanh IMemoryCache; sau nay co the swap sang Redis.
// =============================================================================
namespace PharmaIntel.Core.Interfaces.Services;

public interface IRagCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default);
}
