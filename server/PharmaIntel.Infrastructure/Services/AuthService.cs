// =============================================================================
// Service: AuthService
// Chuc nang: Nghiep vu xac thuc - register/login/refresh/logout/me.
// Refresh token theo single-use rotation (xem RefreshTokenService).
// Su dung PasswordHasher<User> chinh thuc cua ASP.NET Core Identity.
// =============================================================================
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PharmaIntel.Core.DTOs.Auth;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly PharmaIntelDbContext _db;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly GoogleAuthSettings _googleAuth;

    public AuthService(
        PharmaIntelDbContext db,
        IPasswordHasher<User> hasher,
        IJwtTokenService jwt,
        IRefreshTokenService refreshTokens,
        IOptions<GoogleAuthSettings> googleAuth)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
        _refreshTokens = refreshTokens;
        _googleAuth = googleAuth.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ip, string? userAgent, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existing = await _db.Users.AnyAsync(u => u.Email == email, ct);
        if (existing)
            throw new ConflictException("Email da duoc su dung");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            AuthProvider = "local",
            IsTermsAccepted = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return await BuildAuthResponseAsync(user, ip, userAgent, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ip, string? userAgent, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
            throw new UnauthorizedAppException("Email hoac mat khau khong dung");

        if (!user.IsActive)
            throw new UnauthorizedAppException("Tai khoan da bi vo hieu hoa");

        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verify == PasswordVerificationResult.Failed)
            throw new UnauthorizedAppException("Email hoac mat khau khong dung");

        if (verify == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _hasher.HashPassword(user, request.Password);
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return await BuildAuthResponseAsync(user, ip, userAgent, ct);
    }

    public async Task<AuthResponse> LoginWithGoogleAsync(GoogleLoginRequest request, string? ip, string? userAgent, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
            throw new UnauthorizedAppException("Thieu Google ID token");

        if (string.IsNullOrWhiteSpace(_googleAuth.ClientId))
            throw new InvalidOperationException("Google:ClientId chua duoc cau hinh tren server");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(
                request.IdToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    // Bat buoc audience trung Client ID cua app -> chong token tu app khac
                    Audience = new[] { _googleAuth.ClientId }
                });
        }
        catch (InvalidJwtException ex)
        {
            throw new UnauthorizedAppException($"Google token khong hop le: {ex.Message}");
        }

        if (!payload.EmailVerified)
            throw new UnauthorizedAppException("Email Google chua duoc xac thuc");

        var email = payload.Email.Trim().ToLowerInvariant();
        var googleSub = payload.Subject; // unique Google user id

        // Tim user theo Google sub truoc (tranh truong hop user doi email tren Google),
        // sau do fallback theo email (link tai khoan local sang Google).
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.AuthProvider == "google" && u.AuthProviderId == googleSub, ct)
            ?? await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user is null)
        {
            // Tao moi - khong co PasswordHash (login bang Google)
            user = new User
            {
                FullName = string.IsNullOrWhiteSpace(payload.Name) ? email : payload.Name,
                Email = email,
                AvatarUrl = payload.Picture,
                AuthProvider = "google",
                AuthProviderId = googleSub,
                IsTermsAccepted = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);
        }
        else
        {
            if (!user.IsActive)
                throw new UnauthorizedAppException("Tai khoan da bi vo hieu hoa");

            // Tai khoan local truoc do dang ky cung email -> link sang Google.
            // Giu nguyen PasswordHash de user van co the dang nhap local neu muon.
            var changed = false;
            if (user.AuthProvider == "local")
            {
                user.AuthProvider = "google";
                user.AuthProviderId = googleSub;
                changed = true;
            }
            else if (string.IsNullOrEmpty(user.AuthProviderId))
            {
                user.AuthProviderId = googleSub;
                changed = true;
            }
            if (string.IsNullOrWhiteSpace(user.AvatarUrl) && !string.IsNullOrWhiteSpace(payload.Picture))
            {
                user.AvatarUrl = payload.Picture;
                changed = true;
            }
            if (changed)
            {
                user.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
            }
        }

        return await BuildAuthResponseAsync(user, ip, userAgent, ct);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, string? ip, string? userAgent, CancellationToken ct = default)
    {
        var (newRefresh, refreshExpiresAt, userId) =
            await _refreshTokens.RotateAsync(request.RefreshToken, ip, userAgent, ct);

        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new UnauthorizedAppException("Tai khoan khong ton tai");

        if (!user.IsActive)
            throw new UnauthorizedAppException("Tai khoan da bi vo hieu hoa");

        var (accessToken, expiresIn) = _jwt.GenerateAccessToken(user);
        return new AuthResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = expiresIn,
            RefreshToken = newRefresh,
            RefreshTokenExpiresAt = refreshExpiresAt,
            User = MapUserInfo(user)
        };
    }

    public Task LogoutAsync(LogoutRequest request, string? ip, CancellationToken ct = default)
        => _refreshTokens.RevokeAsync(request.RefreshToken, ip, ct);

    public async Task<UserInfo?> GetMeAsync(long userId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        return user is null ? null : MapUserInfo(user);
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(User user, string? ip, string? userAgent, CancellationToken ct)
    {
        var (accessToken, expiresIn) = _jwt.GenerateAccessToken(user);
        var (refreshToken, refreshExpiresAt) = await _refreshTokens.IssueAsync(user.Id, ip, userAgent, ct);
        return new AuthResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = expiresIn,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt,
            User = MapUserInfo(user)
        };
    }

    private static UserInfo MapUserInfo(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        AvatarUrl = user.AvatarUrl,
        PhoneNumber = user.PhoneNumber,
        DateOfBirth = user.DateOfBirth,
        AuthProvider = user.AuthProvider,
        Role = user.Role,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}
