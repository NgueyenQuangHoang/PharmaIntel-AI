// =============================================================================
// Entity: DiagnosticSessionSymptom (Trieu chung trong phien)
// Chuc nang: Bang trung gian N:N giua DiagnosticSession va Symptom.
// Quan he: N:1 voi DiagnosticSession | N:1 voi Symptom.
// Rang buoc: Unique (session_id + symptom_id).
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class DiagnosticSessionSymptom
{
    public long Id { get; set; }
    public long SessionId { get; set; }
    public long SymptomId { get; set; }

    public DiagnosticSession Session { get; set; } = null!;
    public Symptom Symptom { get; set; } = null!;
}
