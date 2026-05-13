// =============================================================================
// DTO: RagEvaluationResultDto
// Chuc nang: Ket qua eval cho 1 test case. Tap ket qua duoc tra ve qua admin
//            endpoint /api/admin/rag-evaluation/run.
// =============================================================================
namespace PharmaIntel.Core.DTOs.RagEvaluation;

public class RagEvaluationResultDto
{
    public string CaseId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public List<string> Failures { get; set; } = new();
    public string AiResponse { get; set; } = string.Empty;
    public int MedicationContextCount { get; set; }
    public int KnowledgeContextCount { get; set; }
}
