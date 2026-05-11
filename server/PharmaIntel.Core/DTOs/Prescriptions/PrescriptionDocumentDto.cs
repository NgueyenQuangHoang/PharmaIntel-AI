// =============================================================================
// DTO: PrescriptionDocumentDto
// Chuc nang: Tra ve thong tin file don thuoc da upload cua user.
// FileUrl la relative path (vd: "/uploads/prescriptions/...") - FE dung baseUrl
// cua API de truy cap.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Prescriptions;

public class PrescriptionDocumentDto
{
    public long Id { get; set; }
    public long PrescriptionId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string VerificationStatus { get; set; } = "pending"; // pending, verified, rejected
    public long? VerifiedByPharmacistId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
