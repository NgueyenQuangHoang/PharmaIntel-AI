// =============================================================================
// Entity: KnowledgeDocument
// Chuc nang: Tai lieu y te / FAQ / huong dan / chinh sach dung lam knowledge
//            base cho RAG Phase 2.
// Quan he:
//   - 1:N KnowledgeChunk (mot tai lieu duoc chunk thanh nhieu doan de embed).
//   - KnowledgeChunk.VectorId tro toi point trong Qdrant collection.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class KnowledgeDocument
{
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;

    // faq | guideline | medication_info | policy | safety
    public string SourceType { get; set; } = "faq";

    public string? SourceUrl { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<KnowledgeChunk> Chunks { get; set; } = new List<KnowledgeChunk>();
}
