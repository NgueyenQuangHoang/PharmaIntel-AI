// =============================================================================
// Entity: Doctor (Bac si)
// Chuc nang: Danh muc bac si ke don trong he thong.
// Quan he: 1:N voi Prescription (doctor_id co the null neu bac si ngoai he thong).
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class Doctor
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public string? Specialization { get; set; }
    public string? Hospital { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
