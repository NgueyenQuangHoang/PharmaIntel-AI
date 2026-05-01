// =============================================================================
// Entity: Symptom (Trieu chung)
// Chuc nang: Danh muc trieu chung de nguoi dung chon khi bat dau chan doan AI.
// Quan he: N:N voi DiagnosticSession qua DiagnosticSessionSymptom.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class Symptom
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public int DisplayOrder { get; set; }

    public ICollection<DiagnosticSessionSymptom> SessionSymptoms { get; set; } = new List<DiagnosticSessionSymptom>();
}
