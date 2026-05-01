// =============================================================================
// Controller: UsersController
// Chuc nang: Quan ly profile + settings cua user dang dang nhap (user-scoped).
// Endpoints (tat ca yeu cau JWT):
//   GET  /api/users/me                    xem profile
//   PUT  /api/users/me                    cap nhat fullName/avatarUrl
//   PUT  /api/users/me/change-password    doi mat khau (yeu cau current password)
//   GET  /api/users/me/settings           xem settings (auto-create neu chua co)
//   PUT  /api/users/me/settings           cap nhat settings
// Email IMMUTABLE - khong co endpoint update email.
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.DTOs.Users;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetMe(CancellationToken ct)
        => Ok(await _service.GetProfileAsync(User.GetUserId(), ct));

    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateMe(
        [FromBody] UpdateUserProfileRequest request, CancellationToken ct)
        => Ok(await _service.UpdateProfileAsync(User.GetUserId(), request, ct));

    [HttpPut("me/change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        await _service.ChangePasswordAsync(User.GetUserId(), request, ct);
        return NoContent();
    }

    [HttpGet("me/settings")]
    public async Task<ActionResult<UserSettingsDto>> GetSettings(CancellationToken ct)
        => Ok(await _service.GetSettingsAsync(User.GetUserId(), ct));

    [HttpPut("me/settings")]
    public async Task<ActionResult<UserSettingsDto>> UpdateSettings(
        [FromBody] UpdateUserSettingsRequest request, CancellationToken ct)
        => Ok(await _service.UpdateSettingsAsync(User.GetUserId(), request, ct));
}
