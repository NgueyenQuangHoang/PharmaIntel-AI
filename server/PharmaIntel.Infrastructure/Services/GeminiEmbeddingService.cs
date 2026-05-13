// =============================================================================
// Service: GeminiEmbeddingService
// Chuc nang: Sinh embedding vector qua Gemini API (text-embedding-004 mac dinh,
//            output 768 chieu - khop voi Qdrant.VectorSize=768).
// Quan he:
//   - Implement IEmbeddingService.
//   - Goi REST: POST {BaseUrl}/models/{Model}:embedContent?key={ApiKey}.
//   - Dung GeminiSettings cho ApiKey/BaseUrl; EmbeddingSettings cho Model.
// Phase 5: cache vector vao bang embedding_cache (key = sha256(text) + model)
//   de tranh goi Gemini lap lai. DbContext truy cap qua IServiceScopeFactory
//   vi typed HttpClient la transient con DbContext la scoped.
// =============================================================================
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class GeminiEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;
    private readonly GeminiSettings _gemini;
    private readonly EmbeddingSettings _embedding;
    private readonly IServiceScopeFactory _scopeFactory;

    public GeminiEmbeddingService(
        HttpClient http,
        IOptions<GeminiSettings> gemini,
        IOptions<EmbeddingSettings> embedding,
        IServiceScopeFactory scopeFactory)
    {
        _http = http;
        _gemini = gemini.Value;
        _embedding = embedding.Value;
        _scopeFactory = scopeFactory;
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<float>();

        if (string.IsNullOrWhiteSpace(_gemini.ApiKey))
            throw new InvalidOperationException("Gemini ApiKey chua duoc cau hinh.");

        var textHash = Sha256(text.Trim().ToLowerInvariant());

        // Cache lookup
        using (var scope = _scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PharmaIntelDbContext>();
            var cached = await db.EmbeddingCaches.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TextHash == textHash && x.Model == _embedding.Model, ct);

            if (cached != null)
            {
                var cachedVector = JsonSerializer.Deserialize<float[]>(cached.VectorJson);
                if (cachedVector != null && cachedVector.Length > 0)
                    return cachedVector;
            }
        }

        var url = $"{_gemini.BaseUrl.TrimEnd('/')}/models/{_embedding.Model}:embedContent?key={_gemini.ApiKey}";

        var body = new
        {
            content = new
            {
                parts = new[]
                {
                    new { text }
                }
            }
        };

        using var response = await _http.PostAsJsonAsync(url, body, ct);
        var raw = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Gemini embedding loi {(int)response.StatusCode}: {raw}");

        using var doc = JsonDocument.Parse(raw);

        var values = doc.RootElement
            .GetProperty("embedding")
            .GetProperty("values")
            .EnumerateArray()
            .Select(x => x.GetSingle())
            .ToArray();

        // Cache write - swallow loi neu insert race condition (unique constraint).
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PharmaIntelDbContext>();
            db.EmbeddingCaches.Add(new EmbeddingCache
            {
                TextHash = textHash,
                Model = _embedding.Model,
                VectorJson = JsonSerializer.Serialize(values),
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            // Cache la optimistic - khong fail neu trung key (concurrent insert).
        }

        return values;
    }

    private static string Sha256(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
