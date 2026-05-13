// =============================================================================
// DTO: KnowledgeDocumentDto
// Chuc nang: Output tai lieu knowledge base (admin list/get).
// =============================================================================
namespace PharmaIntel.Core.DTOs.Knowledge;

public class KnowledgeDocumentDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int ChunkCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
