// =============================================================================
// Interface: IRagTraceService
// Chuc nang: Log moi lan AI tra loi cung context da retrieve (Phase 3 + 5).
// Phase 5: them latency + errorType.
// =============================================================================
namespace PharmaIntel.Core.Interfaces.Services;

public interface IRagTraceService
{
    Task LogAsync(
        long? diagnosticSessionId,
        string userMessage,
        IReadOnlyList<AiMedicationContext> medicationContexts,
        IReadOnlyList<KnowledgeContext> knowledgeContexts,
        string aiResponse,
        int retrievalLatencyMs = 0,
        int generationLatencyMs = 0,
        int totalLatencyMs = 0,
        string? errorType = null,
        CancellationToken ct = default);
}
