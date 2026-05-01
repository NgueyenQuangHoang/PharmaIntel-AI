// =============================================================================
// Entity: DiagnosticResult (Ket qua chan doan AI)
// Chuc nang: Luu ket qua AI tra ve (ket luan, do tin cay, muc do nguy co, red flags).
// Quan he: 1:1 voi DiagnosticSession | N:1 voi User | 1:N voi DiagnosticResultMedication.
// Luu y: Luu model_name + model_version de audit va truy vet.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class DiagnosticResult
{
    public long Id { get; set; }
    public long SessionId { get; set; }
    public long UserId { get; set; }
    public string? AiConclusion { get; set; }
    public decimal ConfidenceScore { get; set; }
    public string RiskLevel { get; set; } = "low"; // low, medium, high, emergency
    public string? RedFlags { get; set; }
    public bool RequiresDoctorVisit { get; set; }
    public string? ModelName { get; set; }
    public string? ModelVersion { get; set; }
    public DateTime DiagnosedAt { get; set; } = DateTime.UtcNow;

    public DiagnosticSession Session { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<DiagnosticResultMedication> ResultMedications { get; set; } = new List<DiagnosticResultMedication>();
}
