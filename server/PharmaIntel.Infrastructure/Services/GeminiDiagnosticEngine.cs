// =============================================================================
// Service: GeminiDiagnosticEngine
// Chuc nang: AI engine that su dung Google Gemini API thay cho mock rule-based.
// Quan he:
//   - Implement IDiagnosticEngine (thay the MockDiagnosticEngine trong DI).
//   - Goi REST API: POST {BaseUrl}models/{Model}:generateContent?key={ApiKey}.
//   - Yeu cau Gemini tra ve JSON co cau truc -> parse vao DiagnosticEngineResult.
//   - Van query DB Medications theo keyword (do AI chi biet ten thuoc, khong biet ID).
// Fallback:
//   - Khi Gemini loi (timeout/quota/parse fail) -> rui ro thap, ket luan generic.
//   - Logger ghi lai loi de debug.
// =============================================================================
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class GeminiDiagnosticEngine : IDiagnosticEngine
{
    private const string MODEL_NAME = "Google-Gemini";

    private readonly PharmaIntelDbContext _db;
    private readonly HttpClient _http;
    private readonly GeminiSettings _settings;
    private readonly ILogger<GeminiDiagnosticEngine> _logger;

    public GeminiDiagnosticEngine(
        PharmaIntelDbContext db,
        HttpClient http,
        IOptions<GeminiSettings> settings,
        ILogger<GeminiDiagnosticEngine> logger)
    {
        _db = db;
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<DiagnosticEngineResult> AnalyzeAsync(DiagnosticEngineRequest request, CancellationToken ct = default)
    {
        var symptomList = string.Join(", ", request.SymptomNames ?? []);
        var msgList = request.UserMessages == null || request.UserMessages.Count == 0
            ? "(khong co)"
            : string.Join("\n- ", request.UserMessages);

        var prompt = $@"Ban la tro ly y te (PharmaIntel AI) tai Viet Nam. Phan tich tinh trang nguoi dung va tra ve DUY NHAT mot doan JSON hop le (khong markdown, khong giai thich them).

Trieu chung nguoi dung chon: {symptomList}
Tin nhan mo ta them tu nguoi dung:
- {msgList}

Yeu cau JSON co cau truc:
{{
  ""aiConclusion"": ""Ket luan & khuyen nghi bang tieng Viet, 2-4 cau"",
  ""confidenceScore"": 0-100 (so),
  ""riskLevel"": ""low"" | ""medium"" | ""high"" | ""emergency"",
  ""redFlags"": ""null hoac chuoi mo ta dau hieu nguy hiem"",
  ""requiresDoctorVisit"": true/false,
  ""medicationKeywords"": [""tu khoa de tim thuoc OTC trong catalog, vi du paracetamol, ibuprofen, men tieu hoa, smecta, ho""],
  ""aiReplyMessage"": ""tin nhan ngan 1 cau gui trong chat""
}}

Quy tac:
- Neu co dau hieu nguy hiem (dau nguc, kho tho, ngat, co giat, xuat huyet) -> riskLevel = ""emergency"", requiresDoctorVisit = true.
- Chi de xuat thuoc OTC khong ke don (paracetamol, oresol, smecta, men tieu hoa, vitamin, siro ho...). KHONG de xuat khang sinh.
- KHONG dung dau nhay kep "" trong cac gia tri string (dung dau nhay don ' neu can).
- aiConclusion va aiReplyMessage NGAN GON, moi cai duoi 300 ky tu.
- Tra ve THUAN JSON 1 doan, khong markdown fences, khong xuong dong giua chuoi.";

        string raw = string.Empty;
        try
        {
            raw = await CallGeminiAsync(prompt, ct);
            var parsed = ParseJsonFromResponse(raw);

            var result = new DiagnosticEngineResult
            {
                AiConclusion = parsed.AiConclusion ?? "Khong the phan tich tu Gemini.",
                ConfidenceScore = ClampScore(parsed.ConfidenceScore),
                RiskLevel = NormalizeRisk(parsed.RiskLevel),
                RedFlags = string.IsNullOrWhiteSpace(parsed.RedFlags) ? null : parsed.RedFlags,
                RequiresDoctorVisit = parsed.RequiresDoctorVisit,
                ModelName = MODEL_NAME,
                ModelVersion = _settings.Model,
                AiReplyMessage = parsed.AiReplyMessage
            };

            // Tim Medications phu hop trong catalog theo tu khoa Gemini de xuat
            if (parsed.MedicationKeywords != null && parsed.MedicationKeywords.Count > 0)
            {
                var keywords = parsed.MedicationKeywords
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .Select(k => k.Trim())
                    .Distinct()
                    .Take(8)
                    .ToList();

                if (keywords.Count > 0)
                {
                    result.SuggestedMedicationIds = await _db.Medications.AsNoTracking()
                        .Where(m => m.IsActive
                                    && !m.IsPrescriptionRequired
                                    && keywords.Any(kw => m.Name.Contains(kw)))
                        .OrderByDescending(m => m.IsBestSeller)
                        .ThenByDescending(m => m.StockQuantity)
                        .Take(3)
                        .Select(m => m.Id)
                        .ToListAsync(ct);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini AnalyzeAsync that bai - raw response (truncated): {Raw}",
                raw.Length > 800 ? raw[..800] : raw);
            return new DiagnosticEngineResult
            {
                AiConclusion = "He thong AI tam thoi khong phan tich duoc. Vui long thu lai sau hoac den co so y te neu trieu chung nghiem trong.",
                ConfidenceScore = 30m,
                RiskLevel = "low",
                RequiresDoctorVisit = false,
                ModelName = MODEL_NAME,
                ModelVersion = _settings.Model + "-fallback"
            };
        }
    }

    public string GenerateAutoReply(string userMessage, int existingUserMessageCount)
    {
        // Reply trong chat dung sync rule-based de khong block. Ket luan AI day du chay trong CompleteSession.
        if (existingUserMessageCount >= 3)
            return "Da ghi nhan day du thong tin. Ban co the goi POST /complete de nhan ket luan AI tu Gemini.";

        return "Da ghi nhan. Vui long mo ta them ve thoi gian xuat hien, muc do nghiem trong, " +
               "hoac cac trieu chung kem theo de AI danh gia chinh xac hon.";
    }

    // -------------------------------------------------------------------------
    // Gemini REST call
    // -------------------------------------------------------------------------

    private async Task<string> CallGeminiAsync(string prompt, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            throw new InvalidOperationException("Gemini ApiKey chua duoc cau hinh trong appsettings.");

        var url = $"{_settings.BaseUrl.TrimEnd('/')}/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";

        var body = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = prompt } }
                }
            },
            generationConfig = new
            {
                temperature = 0.4,
                topP = 0.9,
                maxOutputTokens = 2048,
                responseMimeType = "application/json"
            }
        };

        using var resp = await _http.PostAsJsonAsync(url, body, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException($"Gemini API loi {(int)resp.StatusCode}: {raw}");

        using var doc = JsonDocument.Parse(raw);
        // candidates[0].content.parts[0].text
        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Gemini tra ve noi dung rong.");

        return text!;
    }

    private static GeminiAnalysisDto ParseJsonFromResponse(string text)
    {
        // Cat bo markdown fences neu Gemini van them dau ngay khi yeu cau JSON thuan
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```"))
        {
            var firstNewline = trimmed.IndexOf('\n');
            if (firstNewline > 0) trimmed = trimmed[(firstNewline + 1)..];
            if (trimmed.EndsWith("```")) trimmed = trimmed[..^3];
            trimmed = trimmed.Trim();
        }

        var dto = JsonSerializer.Deserialize<GeminiAnalysisDto>(trimmed, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        });

        return dto ?? throw new InvalidOperationException("Khong parse duoc JSON tu Gemini.");
    }

    private static decimal ClampScore(decimal s)
    {
        if (s < 0) return 0;
        if (s > 100) return 100;
        return s;
    }

    private static string NormalizeRisk(string? risk)
    {
        var r = (risk ?? "low").Trim().ToLowerInvariant();
        return r switch
        {
            "low" or "medium" or "high" or "emergency" => r,
            _ => "low"
        };
    }

    private class GeminiAnalysisDto
    {
        [JsonPropertyName("aiConclusion")] public string? AiConclusion { get; set; }
        [JsonPropertyName("confidenceScore")] public decimal ConfidenceScore { get; set; }
        [JsonPropertyName("riskLevel")] public string? RiskLevel { get; set; }
        [JsonPropertyName("redFlags")] public string? RedFlags { get; set; }
        [JsonPropertyName("requiresDoctorVisit")] public bool RequiresDoctorVisit { get; set; }
        [JsonPropertyName("medicationKeywords")] public List<string>? MedicationKeywords { get; set; }
        [JsonPropertyName("aiReplyMessage")] public string? AiReplyMessage { get; set; }
    }
}
