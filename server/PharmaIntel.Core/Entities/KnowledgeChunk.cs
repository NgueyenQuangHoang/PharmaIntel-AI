// =============================================================================
// Entity: KnowledgeChunk
// Chuc nang: Mot doan van ban da chunk tu KnowledgeDocument, da duoc embed va
//            upsert vao Qdrant. VectorId la point ID trong collection Qdrant.
// Quan he:
//   - N:1 KnowledgeDocument (cascade delete).
//   - VectorId unique trong bang -> 1-1 voi 1 point Qdrant.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class KnowledgeChunk
{
    public long Id { get; set; }

    public long DocumentId { get; set; }

    public int ChunkIndex { get; set; }

    public string Content { get; set; } = string.Empty;

    // ID point trong Qdrant
    public string VectorId { get; set; } = string.Empty;

    // JSON metadata: language, tags, medicationName, category...
    public string MetadataJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public KnowledgeDocument Document { get; set; } = null!;
}
