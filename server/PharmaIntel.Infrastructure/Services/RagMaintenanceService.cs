// =============================================================================
// Service: RagMaintenanceService (Phase 5)
// Chuc nang:
//   - CheckConsistencyAsync: voi moi chunk SQL, query Qdrant (search 1 vector
//     dummy + filter) hoac dung GET point. De don gian, dung embedding cua
//     chunk content roi search top-1 - neu khong khop chinh vector_id la coi
//     nhu thieu vector. Phien ban don gian: assume Qdrant co the down -> count
//     chunk va goi 1 search test; chi tiet check tung point co the tang chi phi.
//     Mac dinh impl don gian dem chunk va expose API ket qua.
//   - EnqueueReindexAsync: tao RagJob (reindex) cho 1 document.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class RagMaintenanceService : IRagMaintenanceService
{
    private readonly PharmaIntelDbContext _db;

    public RagMaintenanceService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<ConsistencyCheckResult> CheckConsistencyAsync(bool autoReindex, CancellationToken ct = default)
    {
        var totalDocuments = await _db.KnowledgeDocuments.CountAsync(x => x.IsActive, ct);
        var totalChunks = await _db.KnowledgeChunks.CountAsync(ct);

        // Heuristic: phat hien document co IsActive nhung khong co chunk (vector
        // missing hoac chua duoc index). Day la check don gian va re. Check sau
        // toi tung point cua Qdrant co the tu lam o phien ban sau.
        var documentsNeedingReindex = await _db.KnowledgeDocuments
            .Where(x => x.IsActive && !x.Chunks.Any())
            .Select(x => x.Id)
            .ToListAsync(ct);

        var enqueuedJobIds = new List<long>();

        if (autoReindex)
        {
            foreach (var docId in documentsNeedingReindex)
            {
                var jobId = await EnqueueReindexAsync(docId, ct);
                enqueuedJobIds.Add(jobId);
            }
        }

        return new ConsistencyCheckResult
        {
            TotalDocuments = totalDocuments,
            TotalChunksInSql = totalChunks,
            MissingVectorsInQdrant = documentsNeedingReindex.Count,
            DocumentsNeedingReindex = documentsNeedingReindex,
            EnqueuedJobIds = enqueuedJobIds
        };
    }

    public async Task<long> EnqueueReindexAsync(long documentId, CancellationToken ct = default)
    {
        var exists = await _db.KnowledgeDocuments.AnyAsync(x => x.Id == documentId, ct);
        if (!exists)
            throw new NotFoundException("knowledge document", documentId);

        var job = new RagJob
        {
            JobType = "reindex",
            Status = "queued",
            DocumentId = documentId,
            PayloadJson = "{}",
            CreatedAt = DateTime.UtcNow
        };

        _db.RagJobs.Add(job);
        await _db.SaveChangesAsync(ct);

        return job.Id;
    }
}
