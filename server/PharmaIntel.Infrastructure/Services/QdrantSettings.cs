// =============================================================================
// Settings: QdrantSettings
// Chuc nang: Cau hinh ket noi Qdrant vector DB (binding tu appsettings -> "Qdrant").
// =============================================================================
namespace PharmaIntel.Infrastructure.Services;

public class QdrantSettings
{
    public string BaseUrl { get; set; } = "http://localhost:6333";
    public string CollectionName { get; set; } = "pharmaintel_knowledge";
    public int VectorSize { get; set; } = 768;
}
