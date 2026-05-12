// =============================================================================
// Entity: RefreshToken
// Chuc nang: Luu refresh token de gia han access JWT ma khong can re-login.
// Quan he: N:1 voi User. Self-reference qua ReplacedByTokenId de detect token theft.
// Luu y:
//   - TokenHash luu SHA-256 hex cua plaintext, KHONG luu plaintext.
//   - Single-use: moi lan rotate sinh token moi va revoke token cu (RevokedReason='rotated',
//     ReplacedByTokenId = id token moi). Dung lai token da revoke -> theft signal.
//   - Soft revoke (set RevokedAt) thay vi xoa, de audit + theft detection chain.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class RefreshToken
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedByIp { get; set; }
    public string? UserAgent { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public long? ReplacedByTokenId { get; set; }
    public string? RevokedReason { get; set; } // rotated | logout | theft_detected

    public User User { get; set; } = null!;
    public RefreshToken? ReplacedByToken { get; set; }

    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
}
