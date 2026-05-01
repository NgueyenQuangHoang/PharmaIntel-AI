// =============================================================================
// DTO: LoginRequest
// Chuc nang: Du lieu dang nhap. Validation o LoginRequestValidator.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Auth;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
