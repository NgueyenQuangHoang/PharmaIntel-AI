// =============================================================================
// Service: KnowledgeRetrievalService
// Chuc nang: Embed query nguoi dung -> search top-K trong Qdrant -> map ve
//            KnowledgeContext bang cach join voi KnowledgeChunk trong SQL.
//            Chi tra ve chunk thuoc document IsActive=true.
// Phase 5: cache ket qua trong 10 phut bang IRagCacheService (in-memory).
// =============================================================================
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class KnowledgeRetrievalService : IKnowledgeRetrievalService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    private readonly PharmaIntelDbContext _db;
    private readonly IEmbeddingService _embedding;
    private readonly IVectorSearchService _vector;
    private readonly IRagCacheService _cache;

    public KnowledgeRetrievalService(
        PharmaIntelDbContext db,
        IEmbeddingService embedding,
        IVectorSearchService vector,
        IRagCacheService cache)
    {
        _db = db;
        _embedding = embedding;
        _vector = vector;
        _cache = cache;
    }

    public async Task<IReadOnlyList<KnowledgeContext>> SearchAsync(
        string query,
        int topK = 5,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<KnowledgeContext>();

        var cacheKey = "knowledge_search:" + Sha256(query.Trim().ToLowerInvariant() + ":" + topK);
        var cached = await _cache.GetAsync<IReadOnlyList<KnowledgeContext>>(cacheKey, ct);
        if (cached != null)
            return cached;

        var queryVector = await _embedding.EmbedAsync(query, ct);
        var vectorResults = await _vector.SearchAsync(queryVector, topK, ct);

        var vectorIds = vectorResults.Select(x => x.VectorId).ToList();

        if (vectorIds.Count == 0)
        {
            var empty = (IReadOnlyList<KnowledgeContext>)Array.Empty<KnowledgeContext>();
            await _cache.SetAsync(cacheKey, empty, CacheTtl, ct);
            return empty;
        }

        var chunks = await _db.KnowledgeChunks.AsNoTracking()
            .Include(x => x.Document)
            .Where(x => vectorIds.Contains(x.VectorId) && x.Document.IsActive)
            .ToListAsync(ct);

        var results = chunks
            .Select(chunk =>
            {
                var score = vectorResults.FirstOrDefault(x => x.VectorId == chunk.VectorId)?.Score ?? 0;

                return new KnowledgeContext
                {
                    ChunkId = chunk.Id,
                    DocumentId = chunk.DocumentId,
                    Title = chunk.Document.Title,
                    SourceType = chunk.Document.SourceType,
                    Content = chunk.Content,
                    Score = score
                };
            })
            .OrderByDescending(x => x.Score)
            .ToList();

        var readonly_ = (IReadOnlyList<KnowledgeContext>)results;
        await _cache.SetAsync(cacheKey, readonly_, CacheTtl, ct);
        return readonly_;
    }

    private static string Sha256(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
