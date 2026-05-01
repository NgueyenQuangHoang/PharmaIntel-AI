// =============================================================================
// Interface: IJwtTokenService
// Chuc nang: Tao JWT access token cho user da xac thuc.
// =============================================================================
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IJwtTokenService
{
    (string Token, int ExpiresInSeconds) GenerateAccessToken(User user);
}
