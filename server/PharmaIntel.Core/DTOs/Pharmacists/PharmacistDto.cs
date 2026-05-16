// =============================================================================
// DTO: PharmacistDto
// Chuc nang: Tra ve thong tin duoc si cho client (admin va trang Tu van).
// =============================================================================
namespace PharmaIntel.Core.DTOs.Pharmacists;

public class PharmacistDto
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsOnline { get; set; }
    public bool IsActive { get; set; }
    public int ExperienceYears { get; set; }
    public string? About { get; set; }
    public decimal Rating { get; set; }
    public int ReviewsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
