// =============================================================================
// Interface: IAuthService
// Chuc nang: Hop dong nghiep vu xac thuc - register, login, lay thong tin user.
// =============================================================================
using PharmaIntel.Core.DTOs.Auth;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<UserInfo?> GetMeAsync(long userId, CancellationToken ct = default);
}
