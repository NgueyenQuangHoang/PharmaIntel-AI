// =============================================================================
// Interface: IUserService
// Chuc nang: Hop dong quan ly profile + settings cua user dang dang nhap.
// Email immutable; doi mat khau yeu cau current password.
// Settings auto-create neu user chua co (1:1 lazy provisioning).
// =============================================================================
using PharmaIntel.Core.DTOs.Users;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(long userId, CancellationToken ct = default);
    Task<UserProfileDto> UpdateProfileAsync(long userId, UpdateUserProfileRequest request, CancellationToken ct = default);
    Task ChangePasswordAsync(long userId, ChangePasswordRequest request, CancellationToken ct = default);
    Task<UserSettingsDto> GetSettingsAsync(long userId, CancellationToken ct = default);
    Task<UserSettingsDto> UpdateSettingsAsync(long userId, UpdateUserSettingsRequest request, CancellationToken ct = default);
}
