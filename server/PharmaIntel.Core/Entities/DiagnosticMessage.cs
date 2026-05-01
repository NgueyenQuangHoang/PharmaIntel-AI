// =============================================================================
// Entity: DiagnosticMessage (Tin nhan chan doan)
// Chuc nang: Luu tin nhan chat giua nguoi dung va AI trong phien chan doan.
// Quan he: N:1 voi DiagnosticSession.
// sender_type: user | ai | system.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class DiagnosticMessage
{
    public long Id { get; set; }
    public long SessionId { get; set; }
    public string SenderType { get; set; } = string.Empty; // user, ai, system
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public DiagnosticSession Session { get; set; } = null!;
}
