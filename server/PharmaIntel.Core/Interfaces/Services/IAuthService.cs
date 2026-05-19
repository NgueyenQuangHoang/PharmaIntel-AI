// =============================================================================
// Interface: IAuthService
// Chuc nang: Hop dong nghiep vu xac thuc - register, login, lay thong tin user.
// =============================================================================
using PharmaIntel.Core.DTOs.Auth;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ip, string? userAgent, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, string? ip, string? userAgent, CancellationToken ct = default);
    Task<AuthResponse> LoginWithGoogleAsync(GoogleLoginRequest request, string? ip, string? userAgent, CancellationToken ct = default);
    Task<AuthResponse> RefreshAsync(RefreshRequest request, string? ip, string? userAgent, CancellationToken ct = default);
    Task LogoutAsync(LogoutRequest request, string? ip, CancellationToken ct = default);
    Task<UserInfo?> GetMeAsync(long userId, CancellationToken ct = default);
}
