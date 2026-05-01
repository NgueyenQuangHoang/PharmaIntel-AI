// =============================================================================
// DTOs: DiagnosticSessionListItemDto, DiagnosticSessionDto, DiagnosticMessageDto,
//       DiagnosticResultDto, DiagnosticSuggestedMedicationDto.
// Chuc nang: Tra ve thong tin phien chan doan + ket qua AI cho client.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Diagnostics;

public class DiagnosticMessageDto
{
    public long Id { get; set; }
    public long SessionId { get; set; }
    public string SenderType { get; set; } = string.Empty;     // user, ai, system
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

public class DiagnosticSuggestedMedicationDto
{
    public long Id { get; set; }                                // ResultMedication PK (lien ket)
    public long MedicationId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal DiscountPercent { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsPrescriptionRequired { get; set; }
    public int Priority { get; set; }                           // 1 = primary
}

public class DiagnosticResultDto
{
    public long Id { get; set; }
    public long SessionId { get; set; }
    public string? AiConclusion { get; set; }
    public decimal ConfidenceScore { get; set; }                // 0..100
    public string RiskLevel { get; set; } = "low";              // low, medium, high, emergency
    public string? RedFlags { get; set; }
    public bool RequiresDoctorVisit { get; set; }
    public string? ModelName { get; set; }
    public string? ModelVersion { get; set; }
    public DateTime DiagnosedAt { get; set; }
    public List<DiagnosticSuggestedMedicationDto> SuggestedMedications { get; set; } = [];
}

public class DiagnosticSessionListItemDto
{
    public long Id { get; set; }
    public string Status { get; set; } = "in_progress";
    public int MessageCount { get; set; }
    public List<string> Symptoms { get; set; } = [];            // ten trieu chung da chon
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class DiagnosticSessionDto : DiagnosticSessionListItemDto
{
    public List<DiagnosticMessageDto> Messages { get; set; } = [];
    public DiagnosticResultDto? Result { get; set; }            // null neu chua complete
}
