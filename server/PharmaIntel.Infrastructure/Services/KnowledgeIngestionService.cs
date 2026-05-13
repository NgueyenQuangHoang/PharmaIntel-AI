// =============================================================================
// Service: KnowledgeIngestionService
// Chuc nang: Ingest tai lieu y te vao knowledge base:
//   1. Tao KnowledgeDocument trong SQL.
//   2. Chunk noi dung (CHUNK_SIZE ky tu, overlap CHUNK_OVERLAP).
//   3. Voi moi chunk: luu KnowledgeChunk + embed -> upsert Qdrant.
// Quan he:
//   - Implement IKnowledgeIngestionService.
//   - Dung IEmbeddingService de sinh vector, IVectorSearchService de upsert.
// =============================================================================
using System.Text.Json;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class KnowledgeIngestionService : IKnowledgeIngestionService
{
    private const int CHUNK_SIZE = 1200;
    private const int CHUNK_OVERLAP = 200;

    private readonly PharmaIntelDbContext _db;
    private readonly IEmbeddingService _embedding;
    private readonly IVectorSearchService _vector;

    public KnowledgeIngestionService(
        PharmaIntelDbContext db,
        IEmbeddingService embedding,
        IVectorSearchService vector)
    {
        _db = db;
        _embedding = embedding;
        _vector = vector;
    }

    public async Task<long> IngestTextAsync(
        string title,
        string sourceType,
        string content,
        string? sourceUrl = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required.", nameof(content));

        await _vector.EnsureCollectionAsync(ct);

        var document = new KnowledgeDocument
        {
            Title = title.Trim(),
            SourceType = string.IsNullOrWhiteSpace(sourceType) ? "faq" : sourceType.Trim(),
            SourceUrl = sourceUrl,
            IsActive = true
        };

        _db.KnowledgeDocuments.Add(document);
        await _db.SaveChangesAsync(ct);

        var chunks = SplitIntoChunks(content);

        for (var i = 0; i < chunks.Count; i++)
        {
            var chunkText = chunks[i];
            var vectorId = $"knowledge_{document.Id}_{i}";

            var chunk = new KnowledgeChunk
            {
                DocumentId = document.Id,
                ChunkIndex = i,
                Content = chunkText,
                VectorId = vectorId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    title = document.Title,
                    sourceType = document.SourceType,
                    sourceUrl = document.SourceUrl
                })
            };

            _db.KnowledgeChunks.Add(chunk);
            await _db.SaveChangesAsync(ct);

            var embedding = await _embedding.EmbedAsync(chunkText, ct);

            await _vector.UpsertAsync(
                vectorId,
                embedding,
                new Dictionary<string, object?>
                {
                    ["chunkId"] = chunk.Id,
                    ["documentId"] = document.Id,
                    ["title"] = document.Title,
                    ["sourceType"] = document.SourceType,
                    ["content"] = chunk.Content
                },
                ct);
        }

        return document.Id;
    }

    private static List<string> SplitIntoChunks(string text)
    {
        var normalized = text.Replace("\r\n", "\n").Trim();
        var chunks = new List<string>();

        var start = 0;

        while (start < normalized.Length)
        {
            var length = Math.Min(CHUNK_SIZE, normalized.Length - start);
            var chunk = normalized.Substring(start, length).Trim();

            if (!string.IsNullOrWhiteSpace(chunk))
                chunks.Add(chunk);

            if (start + length >= normalized.Length)
                break;

            start += CHUNK_SIZE - CHUNK_OVERLAP;
        }

        return chunks;
    }
}
