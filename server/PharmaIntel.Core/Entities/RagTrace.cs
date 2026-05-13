// =============================================================================
// Entity: RagTrace
// Chuc nang: Log moi lan chat bot tra loi - bao gom context da retrieve
//            (medication + knowledge) va dac diem cua AI response. Dung de
//            debug hallucination, audit, va tinh metric trong RagEvaluationService.
// Quan he:
//   - DiagnosticSessionId optional (chat tu do van log duoc).
//   - FK SetNull khi session bi xoa de giu lich su trace.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class RagTrace
{
    public long Id { get; set; }

    public long? DiagnosticSessionId { get; set; }

    public string UserMessage { get; set; } = string.Empty;

    // JSON list medication IDs retrieved
    public string MedicationContextJson { get; set; } = "[]";

    // JSON list knowledge chunk IDs retrieved
    public string KnowledgeContextJson { get; set; } = "[]";

    public string AiResponse { get; set; } = string.Empty;

    public bool HasMedicationContext { get; set; }

    public bool HasKnowledgeContext { get; set; }

    public bool HasRedFlagWarning { get; set; }

    public bool HasSuggestedMedication { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DiagnosticSession? DiagnosticSession { get; set; }
}
