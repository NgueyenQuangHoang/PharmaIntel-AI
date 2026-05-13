// =============================================================================
// DTO: RagDashboardDto
// Chuc nang: Metric tong hop chat luong RAG (Phase 4 + 5).
// =============================================================================
namespace PharmaIntel.Core.DTOs.RagDashboard;

public class RagDashboardDto
{
    public int TotalTraces { get; set; }
    public int TotalFeedbacks { get; set; }
    public int NegativeFeedbacks { get; set; }
    public int UnreviewedNegativeFeedbacks { get; set; }
    public decimal NegativeFeedbackRate { get; set; }
    public int NoContextResponses { get; set; }
    public int MedicationContextResponses { get; set; }
    public int KnowledgeContextResponses { get; set; }

    // Phase 5: latency + jobs + cache
    public int AvgRetrievalLatencyMs { get; set; }
    public int AvgGenerationLatencyMs { get; set; }
    public int AvgTotalLatencyMs { get; set; }
    public int P95TotalLatencyMs { get; set; }
    public int FailedJobs { get; set; }
    public int QueuedJobs { get; set; }
    public int EmbeddingCacheSize { get; set; }
}
