// =============================================================================
// Service: CloudinaryImageUploadService
// Chuc nang: Upload anh len Cloudinary su dung CloudinaryDotNet SDK.
// Config lay tu section "Cloudinary" trong appsettings.json.
// =============================================================================
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.Infrastructure.Services;

public class CloudinaryImageUploadService : IImageUploadService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryImageUploadService(IOptions<CloudinarySettings> options)
    {
        var s = options.Value;
        var account = new Account(s.CloudName, s.ApiKey, s.ApiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true; // Luon dung HTTPS
    }

    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string folder,
        CancellationToken ct = default)
    {
        // Lay extension de biet content type
        var ext = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = folder,
            // Overwrite neu cung public_id (khong dung o day vi dung UniqueFilename)
            UniqueFilename = true,
            // Tu dong detect format tu stream
            Format = ext is "jpg" or "jpeg" or "png" or "webp" or "gif" ? ext : "jpg",
        };

        var result = await _cloudinary.UploadAsync(uploadParams, ct);

        if (result.Error != null)
            throw new InvalidOperationException($"Cloudinary upload loi: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }
}
