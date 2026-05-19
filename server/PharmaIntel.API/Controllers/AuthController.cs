// =============================================================================
// Controller: AuthController
// Chuc nang: Endpoint xac thuc - register, login, refresh, logout, me. Controller "thin":
//   - Validation: tu dong qua ValidationFilter (FluentValidation)
//   - Exception: tu dong qua GlobalExceptionHandler -> ProblemDetails
//   - Khong co try/catch trong controller
// Endpoints:
//   POST /api/auth/register
//   POST /api/auth/login
//   POST /api/auth/refresh  (khong yeu cau JWT - access token co the het han)
//   POST /api/auth/logout   (yeu cau JWT - revoke refresh token)
//   GET  /api/auth/me       (yeu cau JWT)
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.DTOs.Auth;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _auth.RegisterAsync(request, ClientIp(), UserAgent(), ct);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(request, ClientIp(), UserAgent(), ct);
        return Ok(result);
    }

    // POST /api/auth/google - dang nhap / dang ky bang Google ID Token
    // Frontend lay credential tu Google Identity Services roi gui len day.
    [HttpPost("google")]
    public async Task<ActionResult<AuthResponse>> LoginWithGoogle([FromBody] GoogleLoginRequest request, CancellationToken ct)
    {
        var result = await _auth.LoginWithGoogleAsync(request, ClientIp(), UserAgent(), ct);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var result = await _auth.RefreshAsync(request, ClientIp(), UserAgent(), ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        await _auth.LogoutAsync(request, ClientIp(), ct);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserInfo>> Me(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var info = await _auth.GetMeAsync(userId, ct)
                   ?? throw new NotFoundException("nguoi dung", userId);
        return Ok(info);
    }

    private string? ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? UserAgent() => HttpContext.Request.Headers.UserAgent.ToString();
}
