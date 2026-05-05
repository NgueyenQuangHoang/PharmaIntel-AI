// =============================================================================
// Interface: IAdminUserService
// Chuc nang: Hop dong cac thao tac quan tri user (admin only):
//   - List + filter user
//   - Doi role giua "user" va "admin"
//   - Lock/unlock account (set IsActive)
//   - Soft delete (deactivate, anonymize email)
// Cac thao tac deu tu choi neu user lam thao tac chinh ban than (id == currentUserId).
// =============================================================================
using PharmaIntel.Core.DTOs.Admin;
using PharmaIntel.Core.DTOs.Common;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IAdminUserService
{
    Task<PagedResult<AdminUserDto>> ListAsync(AdminUserListQuery query, CancellationToken ct = default);
    Task<AdminUserDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<AdminUserDto> UpdateRoleAsync(long currentUserId, long targetUserId, UpdateUserRoleRequest req, CancellationToken ct = default);
    Task<AdminUserDto> UpdateStatusAsync(long currentUserId, long targetUserId, UpdateUserStatusRequest req, CancellationToken ct = default);
    Task DeleteAsync(long currentUserId, long targetUserId, CancellationToken ct = default);
}
