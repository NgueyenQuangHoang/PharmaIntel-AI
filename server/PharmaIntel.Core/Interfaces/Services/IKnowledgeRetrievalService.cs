// =============================================================================
// Interface: IKnowledgeRetrievalService
// Chuc nang: Vector search tai lieu y te lien quan voi query nguoi dung
//            (RAG Phase 2 - semantic search).
// =============================================================================
namespace PharmaIntel.Core.Interfaces.Services;

public interface IKnowledgeRetrievalService
{
    Task<IReadOnlyList<KnowledgeContext>> SearchAsync(
        string query,
        int topK = 5,
        CancellationToken ct = default);
}

public class KnowledgeContext
{
    public long ChunkId { get; set; }
    public long DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public decimal Score { get; set; }
}
