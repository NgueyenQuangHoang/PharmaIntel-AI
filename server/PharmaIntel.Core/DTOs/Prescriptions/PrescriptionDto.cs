// =============================================================================
// DTOs: PrescriptionListItemDto, PrescriptionDto, PrescriptionItemDto
// Chuc nang: Tra ve thong tin don thuoc cho client.
// PrescriptionListItemDto = ban gon cho list (khong kem items).
// PrescriptionDto = day du, kem danh sach items.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Prescriptions;

public class PrescriptionItemDto
{
    public long Id { get; set; }
    public long PrescriptionId { get; set; }
    public long? MedicationId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
}

public class PrescriptionListItemDto
{
    public long Id { get; set; }
    public long? DoctorId { get; set; }
    public string? DoctorNameSnapshot { get; set; }
    public string? Title { get; set; }
    public DateOnly? PrescribedDate { get; set; }
    public string Status { get; set; } = "draft";
    public string VerificationStatus { get; set; } = "not_required";
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PrescriptionDto : PrescriptionListItemDto
{
    public long UserId { get; set; }
    public string? UserFullName { get; set; }
    public List<PrescriptionItemDto> Items { get; set; } = [];
    public List<PrescriptionDocumentDto> Documents { get; set; } = [];
}
