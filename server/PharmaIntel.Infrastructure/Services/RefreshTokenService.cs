// =============================================================================
// Service: RefreshTokenService
// Chuc nang: Issue / Rotate (single-use) / Revoke refresh token. Detect theft khi
//            token da revoked bi dung lai -> revoke toan bo active token cua user.
// Quy tac:
//   - Plaintext token = Base64Url(32 random bytes); chi tra ve client 1 lan duy nhat.
//   - DB chi luu SHA-256(token) -> du leak DB cung khong replay duoc.
//   - Rotate trong 1 transaction de tranh race.
// =============================================================================
using System.Data;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly PharmaIntelDbContext _db;
    private readonly JwtSettings _settings;

    public RefreshTokenService(PharmaIntelDbContext db, IOptions<JwtSettings> options)
    {
        _db = db;
        _settings = options.Value;
    }

    public async Task<(string Token, DateTime ExpiresAt)> IssueAsync(
        long userId, string? ip, string? userAgent, CancellationToken ct = default)
    {
        var (plain, hash) = GenerateToken();
        var expiresAt = DateTime.UtcNow.AddDays(_settings.RefreshExpireDays);

        var entity = new RefreshToken
        {
            UserId = userId,
            TokenHash = hash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = Truncate(ip, 64),
            UserAgent = Truncate(userAgent, 512)
        };
        _db.RefreshTokens.Add(entity);
        await _db.SaveChangesAsync(ct);

        return (plain, expiresAt);
    }

    public async Task<(string Token, DateTime ExpiresAt, long UserId)> RotateAsync(
        string presentedToken, string? ip, string? userAgent, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(presentedToken))
            throw new UnauthorizedAppException("Refresh token khong hop le");

        var presentedHash = Hash(presentedToken);

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        var existing = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == presentedHash, ct);

        if (existing is null)
            throw new UnauthorizedAppException("Refresh token khong hop le");

        // Theft detection: token da revoked nhung van bi dung lai
        if (existing.RevokedAt is not null)
        {
            if (existing.ReplacedByTokenId is not null)
            {
                // Token da rotate truoc do, gio bi dung lai -> dau hieu bi steal.
                // Revoke toan bo active token cua user nay.
                var now = DateTime.UtcNow;
                await _db.RefreshTokens
                    .Where(t => t.UserId == existing.UserId
                                && t.RevokedAt == null
                                && t.ExpiresAt > now)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(t => t.RevokedAt, now)
                        .SetProperty(t => t.RevokedByIp, Truncate(ip, 64))
                        .SetProperty(t => t.RevokedReason, "theft_detected"),
                        ct);

                await tx.CommitAsync(ct);
            }
            throw new UnauthorizedAppException("Refresh token khong hop le");
        }

        if (existing.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAppException("Refresh token da het han");

        // Sinh token moi va lien ket chain
        var (plain, hash) = GenerateToken();
        var newExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshExpireDays);

        var newToken = new RefreshToken
        {
            UserId = existing.UserId,
            TokenHash = hash,
            ExpiresAt = newExpiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = Truncate(ip, 64),
            UserAgent = Truncate(userAgent, 512)
        };
        _db.RefreshTokens.Add(newToken);
        await _db.SaveChangesAsync(ct); // de co newToken.Id

        existing.RevokedAt = DateTime.UtcNow;
        existing.RevokedByIp = Truncate(ip, 64);
        existing.RevokedReason = "rotated";
        existing.ReplacedByTokenId = newToken.Id;
        await _db.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);
        return (plain, newExpiresAt, existing.UserId);
    }

    public async Task RevokeAsync(string presentedToken, string? ip, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(presentedToken)) return;

        var hash = Hash(presentedToken);
        var now = DateTime.UtcNow;

        await _db.RefreshTokens
            .Where(t => t.TokenHash == hash && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.RevokedAt, now)
                .SetProperty(t => t.RevokedByIp, Truncate(ip, 64))
                .SetProperty(t => t.RevokedReason, "logout"),
                ct);
    }

    private static (string Plain, string Hash) GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var plain = Base64UrlEncode(bytes);
        return (plain, Hash(plain));
    }

    private static string Hash(string token)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static string? Truncate(string? s, int max)
        => string.IsNullOrEmpty(s) ? s : (s.Length <= max ? s : s[..max]);
}
