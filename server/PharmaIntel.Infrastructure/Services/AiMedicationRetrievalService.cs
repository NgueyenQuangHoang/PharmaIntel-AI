// =============================================================================
// Service: AiMedicationRetrievalService
// Chuc nang: Tim thuoc lien quan trong SQL Server de cung cap context cho Gemini.
//            Day la tang Retrieval cua RAG Muc 1 (SQL Search + Prompt Context).
// Quan he:
//   - Implement IAiMedicationRetrievalService.
//   - DiagnosticService goi truoc khi gui prompt cho IDiagnosticEngine.
//   - Query EF Core tren bang Medications + filter IsActive = true.
// Cach trich xuat keyword:
//   - Tach symptomsSummary + 2 message user gan nhat theo dau cau, dau phay,
//     khoang trang. Loai stop-word va token < 3 ky tu. Lay toi da 8 keyword.
// Cach match:
//   - OR tren Name, GenericName, ActiveIngredients, Benefits, Description (LIKE).
//   - Order by IsBestSeller DESC, StockQuantity DESC.
//   - Take 15 ban ghi de gioi han prompt size.
// Fallback:
//   - Khong tim ra thuoc nao -> tra ve Top 10 thuoc OTC ban chay nhat (best-seller)
//     de Gemini van co context tham khao thay vi rong tay.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class AiMedicationRetrievalService : IAiMedicationRetrievalService
{
    private const int MAX_KEYWORDS = 8;
    private const int MAX_RESULTS = 15;
    private const int FALLBACK_RESULTS = 10;

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
        var keywords = ExtractKeywords(symptomsSummary, conversationMessages);

        if (keywords.Count > 0)
        {
            // EF Core khong dich tot Any() voi closure list -> build dynamic predicate
            var query = _db.Medications.AsNoTracking().Where(m => m.IsActive);

            var predicate = BuildKeywordPredicate(keywords);
            query = query.Where(predicate);

            var results = await query
                .OrderByDescending(m => m.IsBestSeller)
                .ThenByDescending(m => m.StockQuantity)
                .Take(MAX_RESULTS)
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

            if (results.Count > 0)
                return results;
        }

        // Fallback: top OTC best-seller
        return await _db.Medications.AsNoTracking()
            .Where(m => m.IsActive && !m.IsPrescriptionRequired)
            .OrderByDescending(m => m.IsBestSeller)
            .ThenByDescending(m => m.StockQuantity)
            .Take(FALLBACK_RESULTS)
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
