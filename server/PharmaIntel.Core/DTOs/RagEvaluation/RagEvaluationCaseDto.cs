// =============================================================================
// DTO: RagEvaluationCaseDto + RagExpectedDto
// Chuc nang: Test case cho bo eval RAG (Phase 3). Doc tu docs/rag-evaluation-cases.json.
// =============================================================================
namespace PharmaIntel.Core.DTOs.RagEvaluation;

public class RagEvaluationCaseDto
{
    public string Id { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public RagExpectedDto Expected { get; set; } = new();
}

public class RagExpectedDto
{
    public bool ShouldRetrieveMedication { get; set; }
    public bool ShouldRetrieveKnowledge { get; set; }
    public bool ShouldMentionDoctor { get; set; }
    public bool ShouldSuggestEmergency { get; set; }
    public bool MustNotInventMedication { get; set; } = true;
}
