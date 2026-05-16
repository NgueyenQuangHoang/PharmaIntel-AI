// =============================================================================
// DTOs: PharmacistCreateRequest, PharmacistUpdateRequest, PharmacistListQuery
// Chuc nang: Input tu client cho CRUD ho so duoc si tu van.
// =============================================================================
using PharmaIntel.Core.DTOs.Common;

namespace PharmaIntel.Core.DTOs.Pharmacists;

public class PharmacistCreateRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }   // null -> auto sinh PH-yyyyMMdd-xxx
    public string? Specialization { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsOnline { get; set; }
    public bool IsActive { get; set; } = true;
    public int ExperienceYears { get; set; }
    public string? About { get; set; }
    public decimal Rating { get; set; }
    public int ReviewsCount { get; set; }
}

public class PharmacistUpdateRequest
{
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
}

public class PharmacistListQuery : PagedQuery
{
    public string? Q { get; set; }              // tim theo FullName
    public string? Specialization { get; set; } // loc theo chuyen khoa
    public bool? IsOnline { get; set; }
    public bool? IsActive { get; set; }
}
