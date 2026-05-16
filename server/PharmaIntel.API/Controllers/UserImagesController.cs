// =============================================================================
// Controller: UserImagesController
// Chuc nang: User dang nhap upload anh ca nhan (avatar) len Cloudinary.
// Endpoint:
//   POST /api/users/me/images/upload   - nhan multipart/form-data field "file"
//                                        tra ve { "url": "https://res.cloudinary.com/..." }
// Gioi han: Chi nhan file anh (jpg, jpeg, png, webp, gif), toi da 5 MB.
// Folder Cloudinary: "avatars".
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize]
[Route("api/users/me/images")]
public class UserImagesController : ControllerBase
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IImageUploadService _imageUploadService;

    public UserImagesController(IImageUploadService imageUploadService)
    {
        _imageUploadService = imageUploadService;
    }

    /// <summary>
    /// Upload anh dai dien (avatar) cua user hien tai.
    /// Form field: "file" (IFormFile).
    /// Returns: { "url": "https://res.cloudinary.com/..." }
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UserAvatarUploadResponse>> Upload(
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { detail = "Vui lòng chọn file ảnh." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { detail = $"Chỉ chấp nhận định dạng: {string.Join(", ", AllowedExtensions)}." });

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(new { detail = "Dung lượng file không được vượt quá 5 MB." });

        await using var stream = file.OpenReadStream();
        var url = await _imageUploadService.UploadAsync(stream, file.FileName, "avatars", ct);

        return Ok(new UserAvatarUploadResponse(url));
    }
}

public record UserAvatarUploadResponse(string Url);
