// =============================================================================
// DTOs: UpdateUserProfileRequest, ChangePasswordRequest, UserSettingsDto, UpdateUserSettingsRequest
// Chuc nang: Input/output cho cac thao tac User Profile va User Settings.
// Validation: o Validators/Users/*.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Users;

public class UpdateUserProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class UserSettingsDto
{
    public string DarkMode { get; set; } = "system";        // light, dark, system
    public string LanguageCode { get; set; } = "vi";        // vi, en
    public bool NotificationEnabled { get; set; } = true;
    public bool ReminderSoundEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdateUserSettingsRequest
{
    public string DarkMode { get; set; } = "system";
    public string LanguageCode { get; set; } = "vi";
    public bool NotificationEnabled { get; set; } = true;
    public bool ReminderSoundEnabled { get; set; } = true;
}
