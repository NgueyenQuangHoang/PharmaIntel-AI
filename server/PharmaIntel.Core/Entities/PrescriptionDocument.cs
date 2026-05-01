// =============================================================================
// Entity: PrescriptionDocument (File don thuoc)
// Chuc nang: Luu anh/PDF don thuoc upload, trang thai xac minh boi duoc si.
// Quan he: N:1 voi Prescription | N:1 voi Pharmacist (nguoi xac minh, nullable).
// Trang thai: pending -> verified / rejected.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class PrescriptionDocument
{
    public long Id { get; set; }
    public long PrescriptionId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string VerificationStatus { get; set; } = "pending"; // pending, verified, rejected
    public long? VerifiedByPharmacistId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Prescription Prescription { get; set; } = null!;
    public Pharmacist? VerifiedByPharmacist { get; set; }
}
