// =============================================================================
// Interface: IRagMaintenanceService
// Chuc nang: Cac thao tac bao tri RAG (Phase 5): consistency check SQL <-> Qdrant.
// =============================================================================
namespace PharmaIntel.Core.Interfaces.Services;

public interface IRagMaintenanceService
{
    Task<ConsistencyCheckResult> CheckConsistencyAsync(bool autoReindex, CancellationToken ct = default);

    Task<long> EnqueueReindexAsync(long documentId, CancellationToken ct = default);
}

public class ConsistencyCheckResult
{
    public int TotalDocuments { get; set; }
    public int TotalChunksInSql { get; set; }
    public int MissingVectorsInQdrant { get; set; }
    public List<long> DocumentsNeedingReindex { get; set; } = new();
    public List<long> EnqueuedJobIds { get; set; } = new();
}
