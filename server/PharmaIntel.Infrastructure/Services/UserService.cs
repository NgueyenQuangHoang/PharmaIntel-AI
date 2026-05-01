// =============================================================================
// Service: UserService
// Chuc nang: Quan ly profile + settings cua user dang dang nhap.
// Quy tac:
//   - Email IMMUTABLE - chi hien thi, khong cho update.
//   - ChangePassword: verify CurrentPassword bang IPasswordHasher truoc khi doi.
//   - UserSetting auto-create lan dau (lazy provisioning) vi RegisterAsync khong tao.
//   - JWT claim FullName se stale sau khi update; user can re-login de refresh token.
// =============================================================================
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Users;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly PharmaIntelDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public UserService(PharmaIntelDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<UserProfileDto> GetProfileAsync(long userId, CancellationToken ct = default)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("nguoi dung", userId);
        return ToProfileDto(user);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(long userId, UpdateUserProfileRequest req, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("nguoi dung", userId);

        user.FullName = req.FullName.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(req.AvatarUrl) ? null : req.AvatarUrl.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToProfileDto(user);
    }

    public async Task ChangePasswordAsync(long userId, ChangePasswordRequest req, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("nguoi dung", userId);

        if (string.IsNullOrEmpty(user.PasswordHash))
            throw new ConflictException("Tai khoan dang nhap qua " + user.AuthProvider + " - khong the doi mat khau");

        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, req.CurrentPassword);
        if (verify == PasswordVerificationResult.Failed)
            throw new ValidationException("currentPassword", "Mat khau hien tai khong dung");

        user.PasswordHash = _hasher.HashPassword(user, req.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<UserSettingsDto> GetSettingsAsync(long userId, CancellationToken ct = default)
    {
        var setting = await _db.UserSettings.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId, ct);
        if (setting == null)
        {
            var userExists = await _db.Users.AnyAsync(u => u.Id == userId, ct);
            if (!userExists) throw new NotFoundException("nguoi dung", userId);
            setting = await CreateDefaultSettingsAsync(userId, ct);
        }
        return ToSettingsDto(setting);
    }

    public async Task<UserSettingsDto> UpdateSettingsAsync(long userId, UpdateUserSettingsRequest req, CancellationToken ct = default)
    {
        var setting = await _db.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId, ct);
        if (setting == null)
        {
            var userExists = await _db.Users.AnyAsync(u => u.Id == userId, ct);
            if (!userExists) throw new NotFoundException("nguoi dung", userId);
            setting = await CreateDefaultSettingsAsync(userId, ct);
        }

        setting.DarkMode = req.DarkMode;
        setting.LanguageCode = req.LanguageCode;
        setting.NotificationEnabled = req.NotificationEnabled;
        setting.ReminderSoundEnabled = req.ReminderSoundEnabled;
        setting.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToSettingsDto(setting);
    }

    private async Task<UserSetting> CreateDefaultSettingsAsync(long userId, CancellationToken ct)
    {
        var setting = new UserSetting
        {
            UserId = userId,
            DarkMode = "system",
            LanguageCode = "vi",
            NotificationEnabled = true,
            ReminderSoundEnabled = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.UserSettings.Add(setting);
        await _db.SaveChangesAsync(ct);
        return setting;
    }

    private static UserProfileDto ToProfileDto(User u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email,
        AvatarUrl = u.AvatarUrl,
        AuthProvider = u.AuthProvider,
        Role = u.Role,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt,
        UpdatedAt = u.UpdatedAt
    };

    private static UserSettingsDto ToSettingsDto(UserSetting s) => new()
    {
        DarkMode = s.DarkMode,
        LanguageCode = s.LanguageCode,
        NotificationEnabled = s.NotificationEnabled,
        ReminderSoundEnabled = s.ReminderSoundEnabled,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
