// =============================================================================
// Interface: IRagEvaluationService
// Chuc nang: Chay bo test cases tu docs/rag-evaluation-cases.json va cham diem
//            retrieval + AI response. Phuc vu admin endpoint kiem thu RAG.
// =============================================================================
using PharmaIntel.Core.DTOs.RagEvaluation;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IRagEvaluationService
{
    Task<IReadOnlyList<RagEvaluationResultDto>> RunAsync(CancellationToken ct = default);
}
