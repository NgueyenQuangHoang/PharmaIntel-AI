// =============================================================================
// DTO: AuthResponse
// Chuc nang: Tra ve client sau khi register/login thanh cong - JWT + thong tin user.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
    public UserInfo User { get; set; } = new();
}

public class UserInfo
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string AuthProvider { get; set; } = "local";
    public string Role { get; set; } = "user";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
