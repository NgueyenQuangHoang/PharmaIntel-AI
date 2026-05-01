// =============================================================================
// Entity: DiagnosticResultMedication (Thuoc AI de xuat)
// Chuc nang: Bang trung gian N:N giua DiagnosticResult va Medication (thuoc AI goi y).
// Quan he: N:1 voi DiagnosticResult | N:1 voi Medication.
// Rang buoc: Unique (result_id + medication_id), priority > 0.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class DiagnosticResultMedication
{
    public long Id { get; set; }
    public long ResultId { get; set; }
    public long MedicationId { get; set; }
    public int Priority { get; set; }

    public DiagnosticResult Result { get; set; } = null!;
    public Medication Medication { get; set; } = null!;
}
