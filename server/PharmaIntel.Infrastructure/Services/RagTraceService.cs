// =============================================================================
// Service: RagTraceService
// Chuc nang: Persist RagTrace cho moi lan AI tra loi - context da retrieve,
//            ai response, va cac flag heuristic de eval/audit sau nay.
// =============================================================================
using System.Text.Json;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class RagTraceService : IRagTraceService
{
    private readonly PharmaIntelDbContext _db;

    public RagTraceService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(
        long? diagnosticSessionId,
        string userMessage,
        IReadOnlyList<AiMedicationContext> medicationContexts,
        IReadOnlyList<KnowledgeContext> knowledgeContexts,
        string aiResponse,
        int retrievalLatencyMs = 0,
        int generationLatencyMs = 0,
        int totalLatencyMs = 0,
        string? errorType = null,
        CancellationToken ct = default)
    {
        var medicationContextJson = JsonSerializer.Serialize(
            medicationContexts.Select(x => new
            {
                x.Id,
                x.Name,
                x.GenericName,
                x.IsPrescriptionRequired
            }));

        var knowledgeContextJson = JsonSerializer.Serialize(
            knowledgeContexts.Select(x => new
            {
                x.ChunkId,
                x.DocumentId,
                x.Title,
                x.SourceType,
                x.Score
            }));

        var trace = new RagTrace
        {
            DiagnosticSessionId = diagnosticSessionId,
            UserMessage = userMessage,
            MedicationContextJson = medicationContextJson,
            KnowledgeContextJson = knowledgeContextJson,
            AiResponse = aiResponse,
            HasMedicationContext = medicationContexts.Count > 0,
            HasKnowledgeContext = knowledgeContexts.Count > 0,
            HasSuggestedMedication = medicationContexts.Count > 0 && MentionsMedication(aiResponse, medicationContexts),
            HasRedFlagWarning = ContainsRedFlagWarning(aiResponse),
            RetrievalLatencyMs = retrievalLatencyMs,
            GenerationLatencyMs = generationLatencyMs,
            TotalLatencyMs = totalLatencyMs,
            ErrorType = errorType
        };

        _db.RagTraces.Add(trace);
        await _db.SaveChangesAsync(ct);
    }

    private static bool MentionsMedication(string response, IReadOnlyList<AiMedicationContext> meds)
    {
        if (string.IsNullOrWhiteSpace(response))
            return false;

        return meds.Any(m =>
            !string.IsNullOrWhiteSpace(m.Name)
            && response.Contains(m.Name, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsRedFlagWarning(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return false;

        var text = response.ToLowerInvariant();

        return text.Contains("đi khám")
            || text.Contains("cơ sở y tế")
            || text.Contains("cấp cứu")
            || text.Contains("bác sĩ")
            || text.Contains("khó thở")
            || text.Contains("đau ngực");
    }
}
