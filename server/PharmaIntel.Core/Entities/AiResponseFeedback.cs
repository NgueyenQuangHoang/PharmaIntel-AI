// =============================================================================
// Entity: AiResponseFeedback
// Chuc nang: User danh gia cau tra loi AI (thumbs_up/thumbs_down + reason).
//            Admin review feedback xau de cai thien tai lieu/prompt.
// Quan he:
//   - N:1 User (cascade delete).
//   - DiagnosticSessionId/MessageId/RagTraceId la soft pointer (long?) -> KHONG
//     FK de tranh cascade phuc tap khi xoa session/trace lich su.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class AiResponseFeedback
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long? DiagnosticSessionId { get; set; }

    public long? DiagnosticMessageId { get; set; }

    public long? RagTraceId { get; set; }

    // thumbs_up | thumbs_down
    public string Rating { get; set; } = string.Empty;

    // wrong_medication | unsafe_advice | not_helpful | hallucination | other
    public string? ReasonType { get; set; }

    public string? Comment { get; set; }

    public bool IsReviewed { get; set; }

    public string? AdminNote { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }

    public User User { get; set; } = null!;
}
