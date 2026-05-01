// =============================================================================
// Options: JwtSettings
// Chuc nang: Map cau hinh "Jwt" trong appsettings.json sang object cho IOptions.
// =============================================================================
namespace PharmaIntel.Infrastructure.Services;

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpireMinutes { get; set; } = 60;
}
