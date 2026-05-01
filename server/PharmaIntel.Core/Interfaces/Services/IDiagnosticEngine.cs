// =============================================================================
// Interface: IDiagnosticEngine
// Chuc nang: Engine sinh ket luan AI tu trieu chung + tin nhan chat.
// Hien tai: MockDiagnosticEngine (rule-based). Tuong lai swap = OpenAI/Azure/Gemini.
// =============================================================================
namespace PharmaIntel.Core.Interfaces.Services;

public class DiagnosticEngineRequest
{
    public List<string> SymptomNames { get; set; } = [];
    public List<string> UserMessages { get; set; } = [];
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
    string GenerateAutoReply(string userMessage, int existingUserMessageCount);
}
