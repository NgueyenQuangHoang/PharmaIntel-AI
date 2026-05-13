// =============================================================================
// Service: RagDashboardService
// Chuc nang: Tinh metric tong hop tu RagTrace + AiResponseFeedback + RagJob
//            + EmbeddingCache (Phase 4 + 5).
// P95 latency tinh trong app (top 1000 trace gan day, sort, lay index 95%).
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.RagDashboard;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class RagDashboardService : IRagDashboardService
{
    private readonly PharmaIntelDbContext _db;

    public RagDashboardService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<RagDashboardDto> GetAsync(CancellationToken ct = default)
    {
        var totalTraces = await _db.RagTraces.CountAsync(ct);
        var totalFeedbacks = await _db.AiResponseFeedbacks.CountAsync(ct);

        var negativeFeedbacks = await _db.AiResponseFeedbacks
            .CountAsync(x => x.Rating == "thumbs_down", ct);

        var unreviewedNegativeFeedbacks = await _db.AiResponseFeedbacks
            .CountAsync(x => x.Rating == "thumbs_down" && !x.IsReviewed, ct);

        var noContextResponses = await _db.RagTraces
            .CountAsync(x => !x.HasMedicationContext && !x.HasKnowledgeContext, ct);

        var medicationContextResponses = await _db.RagTraces
            .CountAsync(x => x.HasMedicationContext, ct);

        var knowledgeContextResponses = await _db.RagTraces
            .CountAsync(x => x.HasKnowledgeContext, ct);

        // Phase 5: latency tu 1000 trace gan day (han che chi phi query)
        var recentLatencies = await _db.RagTraces.AsNoTracking()
            .Where(x => x.TotalLatencyMs > 0)
            .OrderByDescending(x => x.CreatedAt)
            .Take(1000)
            .Select(x => new { x.RetrievalLatencyMs, x.GenerationLatencyMs, x.TotalLatencyMs })
            .ToListAsync(ct);

        var avgRetrievalLatency = recentLatencies.Count == 0 ? 0 : (int)recentLatencies.Average(x => x.RetrievalLatencyMs);
        var avgGenerationLatency = recentLatencies.Count == 0 ? 0 : (int)recentLatencies.Average(x => x.GenerationLatencyMs);
        var avgTotalLatency = recentLatencies.Count == 0 ? 0 : (int)recentLatencies.Average(x => x.TotalLatencyMs);

        int p95TotalLatency = 0;
        if (recentLatencies.Count > 0)
        {
            var sorted = recentLatencies.Select(x => x.TotalLatencyMs).OrderBy(x => x).ToList();
            var index = (int)Math.Ceiling(0.95 * sorted.Count) - 1;
            index = Math.Clamp(index, 0, sorted.Count - 1);
            p95TotalLatency = sorted[index];
        }

        var failedJobs = await _db.RagJobs.CountAsync(x => x.Status == "failed", ct);
        var queuedJobs = await _db.RagJobs.CountAsync(x => x.Status == "queued", ct);
        var embeddingCacheSize = await _db.EmbeddingCaches.CountAsync(ct);

        return new RagDashboardDto
        {
            TotalTraces = totalTraces,
            TotalFeedbacks = totalFeedbacks,
            NegativeFeedbacks = negativeFeedbacks,
            UnreviewedNegativeFeedbacks = unreviewedNegativeFeedbacks,
            NegativeFeedbackRate = totalFeedbacks == 0
                ? 0
                : Math.Round(negativeFeedbacks * 100m / totalFeedbacks, 2),
            NoContextResponses = noContextResponses,
            MedicationContextResponses = medicationContextResponses,
            KnowledgeContextResponses = knowledgeContextResponses,
            AvgRetrievalLatencyMs = avgRetrievalLatency,
            AvgGenerationLatencyMs = avgGenerationLatency,
            AvgTotalLatencyMs = avgTotalLatency,
            P95TotalLatencyMs = p95TotalLatency,
            FailedJobs = failedJobs,
            QueuedJobs = queuedJobs,
            EmbeddingCacheSize = embeddingCacheSize
        };
    }
}
