// =============================================================================
// Service: GeminiEmbeddingService
// Chuc nang: Sinh embedding vector qua Gemini API (text-embedding-004 mac dinh,
//            output 768 chieu - khop voi Qdrant.VectorSize=768).
// Quan he:
//   - Implement IEmbeddingService.
//   - Goi REST: POST {BaseUrl}/models/{Model}:embedContent?key={ApiKey}.
//   - Dung GeminiSettings cho ApiKey/BaseUrl; EmbeddingSettings cho Model.
// =============================================================================
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.Infrastructure.Services;

public class GeminiEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;
    private readonly GeminiSettings _gemini;
    private readonly EmbeddingSettings _embedding;

    public GeminiEmbeddingService(
        HttpClient http,
        IOptions<GeminiSettings> gemini,
        IOptions<EmbeddingSettings> embedding)
    {
        _http = http;
        _gemini = gemini.Value;
        _embedding = embedding.Value;
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<float>();

        if (string.IsNullOrWhiteSpace(_gemini.ApiKey))
            throw new InvalidOperationException("Gemini ApiKey chua duoc cau hinh.");

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

        return values;
    }
}
