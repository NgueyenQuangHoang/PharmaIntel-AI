// =============================================================================
// Controller: AdminUsersController
// Chuc nang: Quan ly nguoi dung danh cho admin (admin only).
// Endpoints (yeu cau JWT + role=admin):
//   GET    /api/admin/users              list co paging + filter (q, role, isActive)
//   GET    /api/admin/users/{id}         chi tiet 1 user (kem totalOrders, totalSpent)
//   PUT    /api/admin/users/{id}/role    doi role giua "user" va "admin"
//   PUT    /api/admin/users/{id}/status  lock/unlock (set IsActive)
//   DELETE /api/admin/users/{id}         hard delete (xoa vinh vien user + cascade)
// Bao ve: cac endpoint mutating tu choi neu admin thao tac chinh ban than.
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.DTOs.Admin;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize(Roles = "admin")]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _service;

    public AdminUsersController(IAdminUserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminUserDto>>> List(
        [FromQuery] AdminUserListQuery query, CancellationToken ct)
        => Ok(await _service.ListAsync(query, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<AdminUserDto>> Get(long id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [HttpPut("{id:long}/role")]
    public async Task<ActionResult<AdminUserDto>> UpdateRole(
        long id, [FromBody] UpdateUserRoleRequest request, CancellationToken ct)
        => Ok(await _service.UpdateRoleAsync(User.GetUserId(), id, request, ct));

    [HttpPut("{id:long}/status")]
    public async Task<ActionResult<AdminUserDto>> UpdateStatus(
        long id, [FromBody] UpdateUserStatusRequest request, CancellationToken ct)
        => Ok(await _service.UpdateStatusAsync(User.GetUserId(), id, request, ct));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(User.GetUserId(), id, ct);
        return NoContent();
    }
}
