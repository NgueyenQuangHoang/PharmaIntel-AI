// =============================================================================
// DTO: RefreshRequest - body cho POST /api/auth/refresh
// =============================================================================
namespace PharmaIntel.Core.DTOs.Auth;

public class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
