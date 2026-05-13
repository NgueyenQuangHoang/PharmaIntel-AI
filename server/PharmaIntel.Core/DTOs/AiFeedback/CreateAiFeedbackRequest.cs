// =============================================================================
// DTO: CreateAiFeedbackRequest
// Chuc nang: Payload user gui feedback cho 1 AI response.
// =============================================================================
namespace PharmaIntel.Core.DTOs.AiFeedback;

public class CreateAiFeedbackRequest
{
    public long? DiagnosticSessionId { get; set; }

    public long? DiagnosticMessageId { get; set; }

    public long? RagTraceId { get; set; }

    public string Rating { get; set; } = string.Empty;

    public string? ReasonType { get; set; }

    public string? Comment { get; set; }
}
