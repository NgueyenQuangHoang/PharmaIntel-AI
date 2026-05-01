// =============================================================================
// DTO: RegisterRequest
// Chuc nang: Du lieu nhan tu client khi dang ky. Validation o RegisterRequestValidator.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Auth;

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsTermsAccepted { get; set; }
}
