// =============================================================================
// Service: AiMedicationRetrievalService
// Chuc nang: Tim thuoc lien quan trong SQL Server de cung cap context cho Gemini.
//            Day la tang Retrieval cua RAG Muc 1 (SQL Search + Prompt Context).
// Quan he:
//   - Implement IAiMedicationRetrievalService.
//   - DiagnosticService goi truoc khi gui prompt cho IDiagnosticEngine.
//   - Query EF Core tren bang Medications + filter IsActive = true.
// Cach trich xuat keyword:
//   - Tach symptomsSummary + message gan nhat theo dau cau, dau phay, khoang trang.
//   - Loai stop-word va token < 3 ky tu. Lay toi da MAX_KEYWORDS keyword.
// Cach match:
//   - Chi retrieve khi cau hoi co intent y te (ShouldRetrieveMedications).
//   - OR tren Name, GenericName, ActiveIngredients, Benefits, Description (LIKE)
//     de lay candidate pool, sau do score theo do lien quan (Name/Generic/Active
//     trong so cao hon Benefits/Usage). Threshold MIN_RELEVANCE_SCORE de loai bo
//     thuoc khong lien quan -> tranh dua thuoc rac vao prompt Gemini.
// Khong fallback:
//   - Khong tim ra thuoc lien quan -> tra ve rong. Khong tu lay best-seller OTC
//     vi do la nguon hallucination (Gemini se goi y thuoc khong lien quan).
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class AiMedicationRetrievalService : IAiMedicationRetrievalService
{
    private const int MAX_KEYWORDS = 14;
    private const int MAX_RESULTS = 15;
    private const int CANDIDATE_POOL_SIZE = 60;
    private const int MIN_RELEVANCE_SCORE = 6;

    // Stopword tieng Viet co ban (khong dau cho khop voi du lieu code-friendly)
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "toi","ban","la","co","khong","mot","hai","ba","nhe","nha","da","duoc",
        "cho","thi","va","hay","hoac","den","tu","cua","trong","ngoai","tren",
        "duoi","vao","ra","ve","nay","do","kia","nhu","rat","qua","khi","luc",
        "neu","ma","de","cung","con","se","dang","tren","gi","ai","sao","the",
        "trieu","chung","thuoc","dung","gi","gay","bi","minh","cam","thay",
        "hoi","khac","gia","tri","cac","nhung","mot","hai","ba","bon","nam"
    };

    private static readonly string[] MedicalIntentTerms =
    {
        "đau", "dau", "nhức", "nhuc", "sốt", "sot", "ho", "cảm", "cam", "cúm", "cum",
        "viêm", "viem", "dị ứng", "di ung", "ngứa", "ngua", "phát ban", "phat ban",
        "tiêu chảy", "tieu chay", "táo bón", "tao bon", "buồn nôn", "buon non",
        "đau bụng", "dau bung", "đau họng", "dau hong", "sổ mũi", "so mui",
        "nghẹt mũi", "nghet mui", "khó thở", "kho tho", "đau ngực", "dau nguc",
        "thuốc", "thuoc", "uống gì", "uong gi", "dùng gì", "dung gi",
        "paracetamol", "ibuprofen", "kháng sinh", "khang sinh"
    };

    private static readonly string[] NonClinicalIntentTerms =
    {
        "đơn hàng", "don hang", "giao hàng", "giao hang", "ship",
        "thanh toán", "thanh toan", "đổi trả", "doi tra",
        "địa chỉ", "dia chi", "đăng nhập", "dang nhap",
        "tài khoản", "tai khoan", "khuyến mãi", "khuyen mai"
    };

    private readonly PharmaIntelDbContext _db;

    public AiMedicationRetrievalService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AiMedicationContext>> SearchRelevantMedicationsAsync(
        string symptomsSummary,
        IReadOnlyList<string> conversationMessages,
        CancellationToken ct = default)
    {
        var combinedText = string.Join(" ",
            new[] { symptomsSummary }.Concat(conversationMessages ?? Array.Empty<string>()));

        if (!ShouldRetrieveMedications(combinedText))
            return Array.Empty<AiMedicationContext>();

        var keywords = ExtractKeywords(symptomsSummary, conversationMessages ?? Array.Empty<string>());

        if (keywords.Count == 0)
            return Array.Empty<AiMedicationContext>();

        var predicate = BuildKeywordPredicate(keywords);

        var candidates = await _db.Medications.AsNoTracking()
            .Where(m => m.IsActive)
            .Where(predicate)
            .OrderByDescending(m => m.IsBestSeller)
            .ThenByDescending(m => m.StockQuantity)
            .Take(CANDIDATE_POOL_SIZE)
            .Select(m => new AiMedicationContext
            {
                Id = m.Id,
                Name = m.Name,
                GenericName = m.GenericName,
                ActiveIngredients = m.ActiveIngredients,
                Benefits = m.Benefits,
                UsageInstructions = m.UsageInstructions,
                Contraindications = m.Contraindications,
                IsPrescriptionRequired = m.IsPrescriptionRequired
            })
            .ToListAsync(ct);

        return candidates
            .Select(m => new { Medication = m, Score = ScoreMedication(m, keywords) })
            .Where(x => x.Score >= MIN_RELEVANCE_SCORE)
            .OrderByDescending(x => x.Score)
            .Take(MAX_RESULTS)
            .Select(x => x.Medication)
            .ToList();
    }

    private static bool ShouldRetrieveMedications(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var normalized = text.ToLowerInvariant();

        var hasMedicalIntent = MedicalIntentTerms.Any(term =>
            normalized.Contains(term, StringComparison.OrdinalIgnoreCase));

        if (!hasMedicalIntent)
            return false;

        var hasNonClinicalIntent = NonClinicalIntentTerms.Any(term =>
            normalized.Contains(term, StringComparison.OrdinalIgnoreCase));

        return !hasNonClinicalIntent || hasMedicalIntent;
    }

    private static int ScoreMedication(AiMedicationContext med, IReadOnlyList<string> keywords)
    {
        var score = 0;

        foreach (var kw in keywords)
        {
            if (Contains(med.Name, kw)) score += 10;
            if (Contains(med.GenericName, kw)) score += 8;
            if (Contains(med.ActiveIngredients, kw)) score += 8;
            if (Contains(med.Benefits, kw)) score += 6;
            if (Contains(med.UsageInstructions, kw)) score += 3;
            if (Contains(med.Contraindications, kw)) score += 1;
        }

        return score;
    }

    private static bool Contains(string? value, string keyword)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Contains(keyword, StringComparison.OrdinalIgnoreCase);
    }

    private static List<string> ExtractKeywords(string summary, IReadOnlyList<string> conversationMessages)
    {
        var raw = new List<string>();
        if (!string.IsNullOrWhiteSpace(summary)) raw.Add(summary);

        // Lay toi da 3 message gan nhat de tranh prompt qua dai
        if (conversationMessages != null)
        {
            foreach (var msg in conversationMessages.Reverse().Take(3))
                if (!string.IsNullOrWhiteSpace(msg)) raw.Add(msg);
        }

        var separators = new[] { ' ', ',', '.', ';', ':', '!', '?', '\n', '\r', '\t', '/', '\\', '(', ')', '"', '\'' };
        var tokens = raw
            .SelectMany(s => s.Split(separators, StringSplitOptions.RemoveEmptyEntries))
            .Select(t => t.Trim().ToLowerInvariant())
            .Where(t => t.Length >= 3 && !StopWords.Contains(t))
            .Distinct()
            .Take(MAX_KEYWORDS)
            .ToList();

        return tokens;
    }

    private static System.Linq.Expressions.Expression<Func<PharmaIntel.Core.Entities.Medication, bool>>
        BuildKeywordPredicate(List<string> keywords)
    {
        // OR cua nhieu Like tren nhieu cot. EF Core dich Contains -> LIKE '%kw%'.
        var param = System.Linq.Expressions.Expression.Parameter(typeof(PharmaIntel.Core.Entities.Medication), "m");
        System.Linq.Expressions.Expression? body = null;

        var stringContains = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;
        string[] cols = { nameof(PharmaIntel.Core.Entities.Medication.Name),
                          nameof(PharmaIntel.Core.Entities.Medication.GenericName),
                          nameof(PharmaIntel.Core.Entities.Medication.ActiveIngredients),
                          nameof(PharmaIntel.Core.Entities.Medication.Benefits),
                          nameof(PharmaIntel.Core.Entities.Medication.Description) };

        foreach (var kw in keywords)
        {
            var kwConst = System.Linq.Expressions.Expression.Constant(kw, typeof(string));
            foreach (var col in cols)
            {
                var prop = System.Linq.Expressions.Expression.Property(param, col);
                // Null-safe: m.Col != null && m.Col.Contains(kw)
                var notNull = System.Linq.Expressions.Expression.NotEqual(prop, System.Linq.Expressions.Expression.Constant(null, typeof(string)));
                var contains = System.Linq.Expressions.Expression.Call(prop, stringContains, kwConst);
                var safe = System.Linq.Expressions.Expression.AndAlso(notNull, contains);
                body = body == null ? safe : System.Linq.Expressions.Expression.OrElse(body, safe);
            }
        }

        body ??= System.Linq.Expressions.Expression.Constant(false);
        return System.Linq.Expressions.Expression.Lambda<Func<PharmaIntel.Core.Entities.Medication, bool>>(body, param);
    }
}
