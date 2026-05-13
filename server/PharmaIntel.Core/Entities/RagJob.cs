// =============================================================================
// Entity: RagJob
// Chuc nang: Job queue cho cac thao tac RAG ton thoi gian (Phase 5).
//   - JobType: ingest | reindex | delete_vector
//   - Status:  queued | running | completed | failed
// Worker (RagJobWorker) poll bang nay moi 5s, lay job queued cu nhat va xu ly.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class RagJob
{
    public long Id { get; set; }

    public string JobType { get; set; } = string.Empty;

    public string Status { get; set; } = "queued";

    public long? DocumentId { get; set; }

    public string PayloadJson { get; set; } = "{}";

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}
