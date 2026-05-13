// =============================================================================
// DTO: RagDashboardDto
// Chuc nang: Metric tong hop chat luong RAG (Phase 4).
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
}
