// =============================================================================
// Interface: IKnowledgeIngestionService
// Chuc nang: Ingest + quan ly tai lieu y te (Phase 2 + Phase 4).
// =============================================================================
using PharmaIntel.Core.DTOs.Knowledge;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IKnowledgeIngestionService
{
    Task<long> IngestTextAsync(
        string title,
        string sourceType,
        string content,
        string? sourceUrl = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<KnowledgeDocumentDto>> ListAsync(CancellationToken ct = default);

    Task<KnowledgeDocumentDto> GetByIdAsync(long id, CancellationToken ct = default);

    Task<KnowledgeDocumentDto> UpdateAndReindexAsync(
        long id,
        UpdateKnowledgeDocumentRequest request,
        CancellationToken ct = default);

    Task ReindexAsync(long id, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);
}
