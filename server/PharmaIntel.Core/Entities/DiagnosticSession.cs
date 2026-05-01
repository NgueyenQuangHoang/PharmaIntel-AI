// =============================================================================
// Entity: DiagnosticSession (Phien chan doan AI)
// Chuc nang: Quan ly phien chan doan AI cua nguoi dung (trang thai, thoi gian).
// Quan he: N:1 voi User | 1:1 voi DiagnosticResult | 1:N voi DiagnosticMessage,
//          DiagnosticSessionSymptom.
// Trang thai: in_progress -> analyzing -> completed / cancelled / failed.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class DiagnosticSession
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Status { get; set; } = "in_progress"; // in_progress, analyzing, completed, cancelled, failed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public User User { get; set; } = null!;
    public DiagnosticResult? Result { get; set; }
    public ICollection<DiagnosticSessionSymptom> SessionSymptoms { get; set; } = new List<DiagnosticSessionSymptom>();
    public ICollection<DiagnosticMessage> Messages { get; set; } = new List<DiagnosticMessage>();
}
