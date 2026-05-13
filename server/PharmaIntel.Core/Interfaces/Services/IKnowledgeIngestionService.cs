// =============================================================================
// Interface: IKnowledgeIngestionService
// Chuc nang: Ingest tai lieu y te -> chunk -> embed -> upsert Qdrant + luu SQL.
// =============================================================================
namespace PharmaIntel.Core.Interfaces.Services;

public interface IKnowledgeIngestionService
{
    Task<long> IngestTextAsync(
        string title,
        string sourceType,
        string content,
        string? sourceUrl = null,
        CancellationToken ct = default);
}
