// =============================================================================
// Service: MockDiagnosticEngine
// Chuc nang: Mock AI engine rule-based (chua tich hop OpenAI/Azure/Gemini that).
// Logic:
//   - Trigger emergency neu co cac trieu chung red-flag (dau nguc, kho tho, ngat, co giat).
//   - Chu de cam cum / hoi chung ho / dau dau / dau bung -> ket luan tuong ung.
//   - Default: low-risk, recommend nghi ngoi + gap bac si neu khong cai thien.
// Suggested medications: query DB top N active Medication ID matching keyword tu ten thuoc.
// Khi swap engine that, chi can register lai DI: services.AddScoped<IDiagnosticEngine, OpenAIDiagnosticEngine>().
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class MockDiagnosticEngine : IDiagnosticEngine
{
    private const string MODEL_NAME = "PharmaIntel-MockEngine";
    private const string MODEL_VERSION = "0.1-rule-based";

    private readonly PharmaIntelDbContext _db;

    public MockDiagnosticEngine(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<DiagnosticEngineResult> AnalyzeAsync(DiagnosticEngineRequest request, CancellationToken ct = default)
    {
        var symptoms = (request.SymptomNames ?? []).Select(s => RemoveDiacritics(s.ToLower())).ToList();
        var hasAny = (params string[] kws) => kws.Any(kw => symptoms.Any(s => s.Contains(kw)));

        DiagnosticEngineResult result;

        // 1. Emergency flags
        if (hasAny("dau nguc", "kho tho", "ngat", "co giat", "lien tuc non", "xuat huyet"))
        {
            result = new DiagnosticEngineResult
            {
                AiConclusion = "Cac trieu chung ban mo ta co dau hieu nguy hiem va can duoc kham xet ngay.",
                ConfidenceScore = 60m,
                RiskLevel = "emergency",
                RedFlags = "Phat hien trieu chung red-flag - lien he 115 hoac den co so y te gan nhat.",
                RequiresDoctorVisit = true
            };
        }
        // 2. Cold / flu cluster
        else if (hasAny("sot", "ho", "so mui", "dau hong", "nhuc moi"))
        {
            result = new DiagnosticEngineResult
            {
                AiConclusion = "Co kha nang ban dang bi cam cum thong thuong (viem ho hap tren). " +
                               "Khuyen nghi: nghi ngoi, uong nhieu nuoc, ha sot khi can.",
                ConfidenceScore = 75m,
                RiskLevel = "low",
                RequiresDoctorVisit = false
            };
        }
        // 3. Headache / migraine
        else if (hasAny("dau dau", "chong mat"))
        {
            result = new DiagnosticEngineResult
            {
                AiConclusion = "Trieu chung dau dau co the do cang thang, thieu ngu hoac mat nuoc. " +
                               "Khuyen nghi: nghi ngoi noi yen tinh, du nuoc, neu keo dai >2 ngay nen kham.",
                ConfidenceScore = 65m,
                RiskLevel = "low",
                RequiresDoctorVisit = false
            };
        }
        // 4. Stomach / digestive
        else if (hasAny("dau bung", "buon non", "tieu chay"))
        {
            result = new DiagnosticEngineResult
            {
                AiConclusion = "Co the la roi loan tieu hoa nhe. Khuyen nghi: an nhe, bu dien giai, " +
                               "tranh do cay nong. Neu trieu chung keo dai >48h hoac kem sot cao, di kham.",
                ConfidenceScore = 70m,
                RiskLevel = "medium",
                RequiresDoctorVisit = false
            };
        }
        // 5. Default low-risk
        else
        {
            result = new DiagnosticEngineResult
            {
                AiConclusion = "Cac trieu chung chua ro rang. Khuyen nghi theo doi them, " +
                               "ghi nhan dien bien moi 12h. Neu nang hon, vui long den co so y te.",
                ConfidenceScore = 50m,
                RiskLevel = "low",
                RequiresDoctorVisit = false
            };
        }

        result.ModelName = MODEL_NAME;
        result.ModelVersion = MODEL_VERSION;

        // Suggest medications: tim trong catalog theo keyword phu hop voi rui ro
        var keywords = ResolveMedicationKeywords(symptoms);
        if (keywords.Length > 0)
        {
            var medIds = await _db.Medications.AsNoTracking()
                .Where(m => m.IsActive
                            && !m.IsPrescriptionRequired
                            && keywords.Any(kw => m.Name.Contains(kw)))
                .OrderByDescending(m => m.IsBestSeller)
                .ThenByDescending(m => m.StockQuantity)
                .Take(3)
                .Select(m => m.Id)
                .ToListAsync(ct);
            result.SuggestedMedicationIds = medIds;
        }

        return result;
    }

    public string GenerateAutoReply(string userMessage, int existingUserMessageCount)
    {
        // Sau 3 lan tra loi, goi y user complete session
        if (existingUserMessageCount >= 3)
            return "Da ghi nhan day du thong tin. Ban co the goi POST /complete de nhan ket luan AI.";

        return "Da ghi nhan. Vui long mo ta them ve thoi gian xuat hien, muc do nghiem trong, " +
               "hoac cac trieu chung kem theo de toi danh gia chinh xac hon.";
    }

    private static string[] ResolveMedicationKeywords(List<string> symptoms)
    {
        bool has(string kw) => symptoms.Any(s => s.Contains(kw));

        if (has("sot") || has("dau dau") || has("nhuc moi"))
            return ["paracetamol", "ibuprofen"];
        if (has("ho") || has("dau hong"))
            return ["ho", "viem hong"];
        if (has("so mui"))
            return ["cam cum", "phong cam"];
        if (has("dau bung") || has("tieu chay"))
            return ["smecta", "berberin", "men tieu hoa"];
        if (has("buon non"))
            return ["domperidon"];

        return [];
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (var c in normalized)
        {
            var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(System.Text.NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
    }
}
