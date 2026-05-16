// =============================================================================
// Settings: CloudinarySettings
// Chuc nang: Cau hinh ket noi Cloudinary tu appsettings.json.
// =============================================================================
namespace PharmaIntel.Infrastructure.Services;

public class CloudinarySettings
{
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
}
