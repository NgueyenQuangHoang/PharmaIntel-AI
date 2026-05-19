// =============================================================================
// Options: GoogleAuthSettings
// Chuc nang: Map cau hinh "Google" trong appsettings.json sang object cho IOptions.
//   - ClientId: OAuth 2.0 Web Client ID lay tu Google Cloud Console.
//     Dung de validate ID Token tu frontend (aud claim phai match).
// =============================================================================
namespace PharmaIntel.Infrastructure.Services;

public class GoogleAuthSettings
{
    public string ClientId { get; set; } = string.Empty;
}
