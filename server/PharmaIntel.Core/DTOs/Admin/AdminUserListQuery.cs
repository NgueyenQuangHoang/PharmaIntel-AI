// =============================================================================
// DTO: AdminUserListQuery
// Chuc nang: Tham so query cho GET /api/admin/users.
// Filter theo tu khoa (q tim trong fullName/email), role, isActive + paging.
// =============================================================================
using PharmaIntel.Core.DTOs.Common;

namespace PharmaIntel.Core.DTOs.Admin;

public class AdminUserListQuery : PagedQuery
{
    public string? Q { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
}
