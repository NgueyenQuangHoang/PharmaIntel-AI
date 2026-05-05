// =============================================================================
// Interface: IDiagnosticEngine
// Chuc nang: Engine sinh ket luan AI tu trieu chung + tin nhan chat.
// Hien tai: GeminiDiagnosticEngine (Google Gemini, RAG voi catalog thuoc DB).
// Quan he:
//   - DiagnosticService goi engine sau khi da retrieve danh sach thuoc lien quan
//     qua IAiMedicationRetrievalService (RAG Muc 1: SQL Search + Prompt Context).
//   - Engine PHAI gioi han goi y trong danh sach MedicationContexts cua request.
// =============================================================================
namespace PharmaIntel.Core.Interfaces.Services;

public class AiMedicationContext
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? ActiveIngredients { get; set; }
    public string? Benefits { get; set; }
    public string? UsageInstructions { get; set; }
    public string? Contraindications { get; set; }
    public bool IsPrescriptionRequired { get; set; }
}

public class DiagnosticEngineRequest
{
    public List<string> SymptomNames { get; set; } = [];
    public List<string> UserMessages { get; set; } = [];
    public List<AiMedicationContext> MedicationContexts { get; set; } = [];
}

public class DiagnosticEngineResult
{
    public string AiConclusion { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }                    // 0..100
    public string RiskLevel { get; set; } = "low";                  // low, medium, high, emergency
    public string? RedFlags { get; set; }
    public bool RequiresDoctorVisit { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string ModelVersion { get; set; } = string.Empty;
    public List<long> SuggestedMedicationIds { get; set; } = [];    // priority theo thu tu trong list
    public string? AiReplyMessage { get; set; }                     // tin nhan AI gui kem (optional)
}

public interface IDiagnosticEngine
{
    Task<DiagnosticEngineResult> AnalyzeAsync(DiagnosticEngineRequest request, CancellationToken ct = default);

    // Sinh tin nhan tra loi cho user trong phien chat. Dung Gemini + danh sach thuoc da retrieve.
    Task<string> GenerateChatReplyAsync(
        string symptomsSummary,
        IReadOnlyList<string> conversationMessages,
        string userMessage,
        IReadOnlyList<AiMedicationContext> medicationContexts,
        CancellationToken ct = default);
}
