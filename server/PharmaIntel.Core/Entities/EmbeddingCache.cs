// =============================================================================
// Entity: EmbeddingCache
// Chuc nang: Cache vector embedding (Phase 5) de tranh goi Gemini lap lai cho
//            cung 1 text. Key = (text_hash, model). VectorJson = JSON array float.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class EmbeddingCache
{
    public long Id { get; set; }

    public string TextHash { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    // JSON array float
    public string VectorJson { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
