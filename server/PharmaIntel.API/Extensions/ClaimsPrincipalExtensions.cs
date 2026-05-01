// =============================================================================
// Extension: ClaimsPrincipalExtensions
// Chuc nang: Helper lay UserId tu JWT claim sub - dung chung cho moi controller
// can identity user dang dang nhap.
// =============================================================================
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PharmaIntel.Core.Exceptions;

namespace PharmaIntel.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static long GetUserId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!long.TryParse(sub, out var id) || id <= 0)
            throw new UnauthorizedAppException("Token khong hop le");

        return id;
    }
}
