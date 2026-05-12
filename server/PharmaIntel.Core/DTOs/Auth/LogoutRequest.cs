// =============================================================================
// DTO: LogoutRequest - body cho POST /api/auth/logout, mang theo refresh token
// can revoke. Access token van bat buoc qua [Authorize].
// =============================================================================
namespace PharmaIntel.Core.DTOs.Auth;

public class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
