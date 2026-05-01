// =============================================================================
// DTOs: PrescriptionCreateRequest, PrescriptionUpdateRequest,
//       PrescriptionItemCreateRequest, PrescriptionItemUpdateRequest,
//       PrescriptionListQuery
// Validation: o Validators/Prescriptions/*.
// =============================================================================
using PharmaIntel.Core.DTOs.Common;

namespace PharmaIntel.Core.DTOs.Prescriptions;

public class PrescriptionCreateRequest
{
    public long? DoctorId { get; set; }
    public string? DoctorNameSnapshot { get; set; }   // free-text neu khong co DoctorId
    public string? Title { get; set; }
    public DateOnly? PrescribedDate { get; set; }
}

public class PrescriptionUpdateRequest
{
    public long? DoctorId { get; set; }
    public string? DoctorNameSnapshot { get; set; }
    public string? Title { get; set; }
    public DateOnly? PrescribedDate { get; set; }
    public string Status { get; set; } = "draft";     // draft|active|completed|cancelled (xem rule transition)
}

public class PrescriptionItemCreateRequest
{
    public long? MedicationId { get; set; }            // null neu nhap free-text
    public string? MedicationName { get; set; }        // bat buoc neu MedicationId null
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
}

public class PrescriptionItemUpdateRequest
{
    public long? MedicationId { get; set; }
    public string? MedicationName { get; set; }
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
}

public class PrescriptionListQuery : PagedQuery
{
    public string? Status { get; set; }                // filter theo status
    public string? Q { get; set; }                     // tim theo title hoac doctor name snapshot
}
