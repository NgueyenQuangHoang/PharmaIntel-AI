// =============================================================================
// Settings: EmbeddingSettings
// Chuc nang: Cau hinh provider/model embedding (binding tu appsettings -> "Embedding").
// =============================================================================
namespace PharmaIntel.Infrastructure.Services;

public class EmbeddingSettings
{
    public string Provider { get; set; } = "Gemini";
    public string Model { get; set; } = "text-embedding-004";
}
