// =============================================================================
// Interface: IImageUploadService
// Chuc nang: Upload anh len Cloudinary, tra ve URL cong khai.
// =============================================================================
namespace PharmaIntel.Core.Interfaces.Services;

public interface IImageUploadService
{
    /// <summary>
    /// Upload mot file anh len Cloudinary.
    /// </summary>
    /// <param name="stream">Stream noi dung file.</param>
    /// <param name="fileName">Ten file goc (de lay extension).</param>
    /// <param name="folder">Thu muc tren Cloudinary (vd: "medications").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>URL cong khai cua anh da upload.</returns>
    Task<string> UploadAsync(Stream stream, string fileName, string folder, CancellationToken ct = default);
}
