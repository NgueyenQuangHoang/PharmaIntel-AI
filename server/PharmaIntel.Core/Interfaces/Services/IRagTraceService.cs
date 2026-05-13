// =============================================================================
// Interface: IRagTraceService
// Chuc nang: Log moi lan AI tra loi cung context da retrieve (Phase 3).
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
        CancellationToken ct = default);
}
