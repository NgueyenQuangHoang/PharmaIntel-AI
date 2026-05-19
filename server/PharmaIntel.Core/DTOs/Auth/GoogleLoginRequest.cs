// =============================================================================
// DTO: GoogleLoginRequest
// Chuc nang: Body cua POST /api/auth/google - chua Google ID Token (JWT) ma
//   frontend nhan duoc tu Google Identity Services sau khi user chon tai khoan.
//   Backend se verify token nay voi Google de lay email + sub (Google user id),
//   sau do tao moi hoac dang nhap user tuong ung.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Auth;

public class GoogleLoginRequest
{
    // ID Token (JWT) tu Google Identity Services - field "credential" trong response
    // cua nut "Sign in with Google" hoac field "id_token" cua flow OAuth chuan.
    public string IdToken { get; set; } = string.Empty;
}
