// =============================================================================
// Service: RagDashboardService
// Chuc nang: Count tu RagTrace + AiResponseFeedback de tra ve metric tong hop.
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
            KnowledgeContextResponses = knowledgeContextResponses
        };
    }
}
