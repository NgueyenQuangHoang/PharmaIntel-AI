// =============================================================================
// DTO: IngestKnowledgeRequest
// Chuc nang: Payload de admin ingest 1 tai lieu y te vao knowledge base.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Knowledge;

public class IngestKnowledgeRequest
{
    public string Title { get; set; } = string.Empty;
    public string SourceType { get; set; } = "faq";
    public string Content { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
}
