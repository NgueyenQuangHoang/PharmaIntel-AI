// =============================================================================
// Interface: IRefreshTokenService
// Chuc nang: Quan ly refresh token lifecycle - issue, rotate (single-use),
//            revoke, va detect token theft khi 1 token revoked bi dung lai.
// =============================================================================
namespace PharmaIntel.Core.Interfaces.Services;

public interface IRefreshTokenService
{
    /// <summary>Sinh refresh token moi cho user (dung khi login/register).</summary>
    Task<(string Token, DateTime ExpiresAt)> IssueAsync(
        long userId, string? ip, string? userAgent, CancellationToken ct = default);

    /// <summary>Rotate refresh token: validate token cu, sinh moi, revoke cu, lien ket chain.
    /// Throw UnauthorizedAppException neu token invalid/expired/revoked.
    /// Neu token revoked-and-replaced bi dung lai -> revoke toan bo active token cua user (theft).</summary>
    Task<(string Token, DateTime ExpiresAt, long UserId)> RotateAsync(
        string presentedToken, string? ip, string? userAgent, CancellationToken ct = default);

    /// <summary>Revoke (logout). Idempotent: khong throw neu token khong ton tai/da revoke.</summary>
    Task RevokeAsync(string presentedToken, string? ip, CancellationToken ct = default);
}
