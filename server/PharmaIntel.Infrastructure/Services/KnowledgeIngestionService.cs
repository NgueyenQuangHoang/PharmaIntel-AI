// =============================================================================
// Service: KnowledgeIngestionService
// Chuc nang: Quan ly knowledge base (Phase 2 + Phase 4).
//   - IngestTextAsync: tao document + chunk + embed + upsert Qdrant.
//   - ListAsync/GetByIdAsync: read API cho admin.
//   - UpdateAndReindexAsync: cap nhat meta + content -> delete chunk cu (SQL +
//     Qdrant) -> chunk + embed + upsert lai.
//   - ReindexAsync: re-embed cac chunk hien co (khi doi model embedding).
//   - DeleteAsync: xoa document + chunks (SQL cascade) + vectors trong Qdrant.
// Quan he:
//   - Implement IKnowledgeIngestionService.
//   - Dung IEmbeddingService + IVectorSearchService.
// =============================================================================
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Knowledge;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
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
            throw new ValidationException("title", "Title is required.");

        if (string.IsNullOrWhiteSpace(content))
            throw new ValidationException("content", "Content is required.");

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

        await ChunkEmbedAndUpsertAsync(document, content, ct);

        return document.Id;
    }

    public async Task<IReadOnlyList<KnowledgeDocumentDto>> ListAsync(CancellationToken ct = default)
    {
        return await _db.KnowledgeDocuments.AsNoTracking()
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new KnowledgeDocumentDto
            {
                Id = x.Id,
                Title = x.Title,
                SourceType = x.SourceType,
                SourceUrl = x.SourceUrl,
                Description = x.Description,
                IsActive = x.IsActive,
                ChunkCount = x.Chunks.Count,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<KnowledgeDocumentDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var dto = await _db.KnowledgeDocuments.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new KnowledgeDocumentDto
            {
                Id = x.Id,
                Title = x.Title,
                SourceType = x.SourceType,
                SourceUrl = x.SourceUrl,
                Description = x.Description,
                IsActive = x.IsActive,
                ChunkCount = x.Chunks.Count,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("knowledge document", id);

        return dto;
    }

    public async Task<KnowledgeDocumentDto> UpdateAndReindexAsync(
        long id,
        UpdateKnowledgeDocumentRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("title", "Title is required.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ValidationException("content", "Content is required.");

        var document = await _db.KnowledgeDocuments
            .Include(x => x.Chunks)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("knowledge document", id);

        document.Title = request.Title.Trim();
        document.SourceType = string.IsNullOrWhiteSpace(request.SourceType) ? "faq" : request.SourceType.Trim();
        document.SourceUrl = request.SourceUrl;
        document.Description = request.Description;
        document.IsActive = request.IsActive;
        document.UpdatedAt = DateTime.UtcNow;

        await DeleteChunksAndVectorsAsync(document.Id, ct);
        await ChunkEmbedAndUpsertAsync(document, request.Content, ct);

        return await GetByIdAsync(document.Id, ct);
    }

    public async Task ReindexAsync(long id, CancellationToken ct = default)
    {
        var document = await _db.KnowledgeDocuments
            .Include(x => x.Chunks)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("knowledge document", id);

        await _vector.EnsureCollectionAsync(ct);

        // Re-embed cac chunk hien co (vd. khi doi model embedding). Khong delete
        // chunk cu trong SQL - chi cap nhat vector trong Qdrant.
        foreach (var chunk in document.Chunks.OrderBy(x => x.ChunkIndex))
        {
            var embedding = await _embedding.EmbedAsync(chunk.Content, ct);

            await _vector.UpsertAsync(
                chunk.VectorId,
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

        document.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var document = await _db.KnowledgeDocuments
            .Include(x => x.Chunks)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("knowledge document", id);

        await DeleteChunksAndVectorsAsync(document.Id, ct);

        _db.KnowledgeDocuments.Remove(document);
        await _db.SaveChangesAsync(ct);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task ChunkEmbedAndUpsertAsync(KnowledgeDocument document, string content, CancellationToken ct)
    {
        await _vector.EnsureCollectionAsync(ct);

        var chunks = SplitIntoChunks(content);

        for (var i = 0; i < chunks.Count; i++)
        {
            var chunkText = chunks[i];
            // Them GUID de tranh trung VectorId voi chunk cua document cu sau khi
            // re-index nhieu lan (unique index UQ_knowledge_chunks_vector_id).
            var vectorId = $"knowledge_{document.Id}_{i}_{Guid.NewGuid():N}";

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

        await _db.SaveChangesAsync(ct);
    }

    private async Task DeleteChunksAndVectorsAsync(long documentId, CancellationToken ct)
    {
        var chunks = await _db.KnowledgeChunks
            .Where(x => x.DocumentId == documentId)
            .ToListAsync(ct);

        foreach (var chunk in chunks)
        {
            try
            {
                await _vector.DeleteAsync(chunk.VectorId, ct);
            }
            catch
            {
                // Bo qua loi Qdrant de van xoa duoc trong SQL.
                // Caller co the goi reindex/cleanup sau de dong bo.
            }
        }

        _db.KnowledgeChunks.RemoveRange(chunks);
        await _db.SaveChangesAsync(ct);
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
