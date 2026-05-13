// =============================================================================
// DTO: AiFeedbackDto
// Chuc nang: Output cua feedback ra cho user va admin.
// =============================================================================
namespace PharmaIntel.Core.DTOs.AiFeedback;

public class AiFeedbackDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long? DiagnosticSessionId { get; set; }
    public long? DiagnosticMessageId { get; set; }
    public long? RagTraceId { get; set; }
    public string Rating { get; set; } = string.Empty;
    public string? ReasonType { get; set; }
    public string? Comment { get; set; }
    public bool IsReviewed { get; set; }
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
