// =============================================================================
// DTO: UpdateKnowledgeDocumentRequest
// Chuc nang: Payload admin update + reindex tai lieu knowledge base.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Knowledge;

public class UpdateKnowledgeDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public string SourceType { get; set; } = "faq";
    public string? SourceUrl { get; set; }
    public string? Description { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
