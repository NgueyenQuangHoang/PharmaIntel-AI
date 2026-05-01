// =============================================================================
// DTO: UserProfileDto
// Chuc nang: Tra ve thong tin profile cua user (khong gom PasswordHash, AuthProviderId).
// Email immutable - chi hien thi, khong cho update qua API profile.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Users;

public class UserProfileDto
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string AuthProvider { get; set; } = "local";
    public string Role { get; set; } = "user";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
