// =============================================================================
// Settings: GeminiSettings
// Chuc nang: Cau hinh ket noi Google Gemini API (binding tu appsettings.json -> "Gemini").
// =============================================================================
namespace PharmaIntel.Infrastructure.Services;

public class GeminiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.0-flash";
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/";
    public int TimeoutSeconds { get; set; } = 30;
    public bool Enabled { get; set; } = true;
}
