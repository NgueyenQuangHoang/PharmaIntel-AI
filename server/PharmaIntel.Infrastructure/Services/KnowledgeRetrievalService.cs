// =============================================================================
// Service: KnowledgeRetrievalService
// Chuc nang: Embed query nguoi dung -> search top-K trong Qdrant -> map ve
//            KnowledgeContext bang cach join voi KnowledgeChunk trong SQL.
//            Chi tra ve chunk thuoc document IsActive=true.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class KnowledgeRetrievalService : IKnowledgeRetrievalService
{
    private readonly PharmaIntelDbContext _db;
    private readonly IEmbeddingService _embedding;
    private readonly IVectorSearchService _vector;

    public KnowledgeRetrievalService(
        PharmaIntelDbContext db,
        IEmbeddingService embedding,
        IVectorSearchService vector)
    {
        _db = db;
        _embedding = embedding;
        _vector = vector;
    }

    public async Task<IReadOnlyList<KnowledgeContext>> SearchAsync(
        string query,
        int topK = 5,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<KnowledgeContext>();

        var queryVector = await _embedding.EmbedAsync(query, ct);
        var vectorResults = await _vector.SearchAsync(queryVector, topK, ct);

        var vectorIds = vectorResults.Select(x => x.VectorId).ToList();

        if (vectorIds.Count == 0)
            return Array.Empty<KnowledgeContext>();

        var chunks = await _db.KnowledgeChunks.AsNoTracking()
            .Include(x => x.Document)
            .Where(x => vectorIds.Contains(x.VectorId) && x.Document.IsActive)
            .ToListAsync(ct);

        return chunks
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
    }
}
