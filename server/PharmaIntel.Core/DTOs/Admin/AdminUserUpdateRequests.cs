// =============================================================================
// DTOs: UpdateUserRoleRequest, UpdateUserStatusRequest
// Chuc nang: Body cho 2 endpoint admin doi role va lock/unlock user.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Admin;

public class UpdateUserRoleRequest
{
    public string Role { get; set; } = "user";
}

public class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
}
