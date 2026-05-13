// =============================================================================
// Service: GeminiDiagnosticEngine
// Chuc nang: AI engine dung Google Gemini API + RAG Muc 1 (SQL Search + Prompt
//            Context). Khong train, khong embedding, khong vector DB.
// Quan he:
//   - Implement IDiagnosticEngine (DI thay MockDiagnosticEngine).
//   - Goi REST: POST {BaseUrl}models/{Model}:generateContent?key={ApiKey}.
//   - Nhan IReadOnlyList<AiMedicationContext> da retrieve san tu DB qua
//     IAiMedicationRetrievalService -> inject vao prompt.
//   - Gemini chi duoc khuyen thuoc co trong danh sach. Backend ALSO loc lai
//     SuggestedMedicationIds trong DiagnosticService de phong Gemini bia ID.
// Fallback:
//   - Loi (timeout/quota/parse) -> ket luan generic, khong goi y thuoc.
// =============================================================================
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.Infrastructure.Services;

public class GeminiDiagnosticEngine : IDiagnosticEngine
{
    private const string MODEL_NAME = "Google-Gemini";

    private readonly HttpClient _http;
    private readonly GeminiSettings _settings;
    private readonly ILogger<GeminiDiagnosticEngine> _logger;

    public GeminiDiagnosticEngine(
        HttpClient http,
        IOptions<GeminiSettings> settings,
        ILogger<GeminiDiagnosticEngine> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    // -------------------------------------------------------------------------
    // AnalyzeAsync: ket luan AI khi Complete session
    // -------------------------------------------------------------------------
    public async Task<DiagnosticEngineResult> AnalyzeAsync(DiagnosticEngineRequest request, CancellationToken ct = default)
    {
        var symptomsSummary = string.Join(", ", request.SymptomNames ?? []);
        var conversation = (request.UserMessages ?? []).Select(m => $"user: {m}").ToList();
        var medsBlock = BuildMedicationContextText(request.MedicationContexts ?? []);

        var prompt = $@"Ban la AI ho tro sang loc trieu chung cho ung dung PharmaIntel tai Viet Nam.

Nhiem vu: Phan tich trieu chung va tra ve DUY NHAT mot doan JSON hop le (khong markdown, khong giai thich them).

Quy tac an toan:
- Khong khang dinh chan doan chac chan.
- Khong thay the bac si hoac duoc si.
- KHONG khuyen nghi thuoc ngoai DANH SACH THUOC HE THONG ben duoi.
- recommendedMedicationIds CHI duoc lay ID tu danh sach thuoc he thong.
- Neu DANH SACH THUOC HE THONG rong hoac khong lien quan, recommendedMedicationIds BAT BUOC la mang rong [].
- Neu khong co thuoc lien quan, advice phai noi ro he thong chua tim thay thuoc phu hop va khuyen hoi bac si/duoc si neu trieu chung keo dai.
- Neu thuoc IsPrescriptionRequired = true, advice phai noi ro can bac si/duoc si xac nhan.
- Neu trieu chung nguy hiem (kho tho, dau nguc, sot cao keo dai, mat y thuc, co giat, dau du doi, xuat huyet) -> requiresDoctorVisit = true va riskLevel = ""emergency"".
- Tra loi bang tieng Viet, ngan gon.

Trieu chung da chon: {symptomsSummary}

DANH SACH THUOC HE THONG (chi duoc chon trong day):
{medsBlock}

Lich su hoi thoai:
{(conversation.Count == 0 ? "(khong co)" : string.Join("\n", conversation))}

Hay tra ve JSON dung format sau:
{{
  ""aiConclusion"": ""ket luan tham khao 2-4 cau"",
  ""confidenceScore"": 0-100,
  ""riskLevel"": ""low|medium|high|emergency"",
  ""redFlags"": null hoac chuoi mo ta dau hieu nguy hiem,
  ""requiresDoctorVisit"": true|false,
  ""recommendedMedicationIds"": [<chi lay ID tu danh sach tren, toi da 3 ID>],
  ""advice"": ""loi khuyen an toan ngan gon"",
  ""aiReplyMessage"": ""tin nhan ngan 1 cau gui trong chat""
}}

Khong dung dau "" trong gia tri string (dung ' neu can). Tra ve THUAN JSON, khong fences.";

        string raw = string.Empty;
        try
        {
            raw = await CallGeminiAsync(prompt, ct);
            var parsed = ParseJsonFromResponse(raw);

            return new DiagnosticEngineResult
            {
                AiConclusion = string.IsNullOrWhiteSpace(parsed.AiConclusion)
                    ? (string.IsNullOrWhiteSpace(parsed.Advice) ? "Khong the phan tich tu Gemini." : parsed.Advice!)
                    : parsed.AiConclusion!,
                ConfidenceScore = ClampScore(parsed.ConfidenceScore),
                RiskLevel = NormalizeRisk(parsed.RiskLevel),
                RedFlags = string.IsNullOrWhiteSpace(parsed.RedFlags) ? null : parsed.RedFlags,
                RequiresDoctorVisit = parsed.RequiresDoctorVisit,
                ModelName = MODEL_NAME,
                ModelVersion = _settings.Model,
                AiReplyMessage = parsed.AiReplyMessage,
                SuggestedMedicationIds = parsed.RecommendedMedicationIds ?? []
            };
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

    // -------------------------------------------------------------------------
    // GenerateChatReplyAsync: tra loi tin nhan trong chat (RAG)
    // -------------------------------------------------------------------------
    public async Task<string> GenerateChatReplyAsync(
        string symptomsSummary,
        IReadOnlyList<string> conversationMessages,
        string userMessage,
        IReadOnlyList<AiMedicationContext> medicationContexts,
        CancellationToken ct = default)
    {
        var medsBlock = BuildMedicationContextText(medicationContexts);
        var history = conversationMessages == null || conversationMessages.Count == 0
            ? "(chua co)"
            : string.Join("\n", conversationMessages);

        var prompt = $@"Ban la tro ly y te AI trong ung dung PharmaIntel.

Quy tac an toan:
- Khong chan doan chac chan, khong thay the bac si/duoc si.
- KHONG bia ten thuoc ngoai DANH SACH THUOC HE THONG ben duoi.
- Chi nhac den thuoc co trong danh sach. Neu thuoc can don, phai noi ro can bac si/duoc si xac nhan.
- Neu DANH SACH THUOC HE THONG rong hoac khong lien quan, KHONG duoc goi y ten thuoc; hay noi he thong chua tim thay thuoc phu hop.
- Khong dua lieu dung ca nhan hoa.
- Neu co dau hieu nguy hiem (kho tho, dau nguc, sot cao keo dai, mat y thuc, co giat, dau du doi), khuyen den co so y te.
- Tra loi NGAN GON, de hieu, bang tieng Viet, toi da 4 cau.

Trieu chung da chon: {symptomsSummary}

DANH SACH THUOC HE THONG LIEN QUAN:
{medsBlock}

Lich su hoi thoai:
{history}

Tin nhan moi cua nguoi dung:
{userMessage}

Hay phan hoi nhu mot tro ly y te can trong. Tra ve thuan van ban, khong markdown.";

        try
        {
            var text = await CallGeminiAsync(prompt, ct, plainText: true);
            return text.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini GenerateChatReplyAsync that bai");
            return "Da ghi nhan. He thong AI tam thoi khong phan tich duoc, vui long thu lai sau hoac mo ta them ve thoi gian xuat hien va muc do nghiem trong.";
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string BuildMedicationContextText(IReadOnlyList<AiMedicationContext> meds)
    {
        if (meds == null || meds.Count == 0)
            return "(He thong khong tim thay thuoc lien quan. Khong duoc goi y thuoc.)";

        var sb = new StringBuilder();
        foreach (var m in meds)
        {
            sb.Append("- [ID=").Append(m.Id).Append("] ").Append(m.Name);
            if (!string.IsNullOrWhiteSpace(m.GenericName))
                sb.Append(" (").Append(m.GenericName).Append(')');
            sb.Append(m.IsPrescriptionRequired ? " [CAN DON]" : " [OTC]");

            if (!string.IsNullOrWhiteSpace(m.ActiveIngredients))
                sb.Append(" | Hoat chat: ").Append(Truncate(m.ActiveIngredients, 80));
            if (!string.IsNullOrWhiteSpace(m.Benefits))
                sb.Append(" | Cong dung: ").Append(Truncate(m.Benefits, 120));

            sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string Truncate(string? s, int max)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return s.Length <= max ? s : s[..max] + "...";
    }

    private async Task<string> CallGeminiAsync(string prompt, CancellationToken ct, bool plainText = false)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            throw new InvalidOperationException("Gemini ApiKey chua duoc cau hinh trong appsettings.");

        var url = $"{_settings.BaseUrl.TrimEnd('/')}/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";

        object generationConfig = plainText
            ? new { temperature = 0.5, topP = 0.9, maxOutputTokens = 1024 }
            : (object)new { temperature = 0.4, topP = 0.9, maxOutputTokens = 2048, responseMimeType = "application/json" };

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
            generationConfig
        };

        using var resp = await _http.PostAsJsonAsync(url, body, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException($"Gemini API loi {(int)resp.StatusCode}: {raw}");

        using var doc = JsonDocument.Parse(raw);
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
        [JsonPropertyName("recommendedMedicationIds")] public List<long>? RecommendedMedicationIds { get; set; }
        [JsonPropertyName("advice")] public string? Advice { get; set; }
        [JsonPropertyName("aiReplyMessage")] public string? AiReplyMessage { get; set; }
    }
}
