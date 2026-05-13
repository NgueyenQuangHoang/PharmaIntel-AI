// =============================================================================
// Service: QdrantVectorSearchService
// Chuc nang: Goi REST API Qdrant (PUT/POST) de create collection, upsert point,
//            va search top-K theo cosine similarity.
// Quan he:
//   - Implement IVectorSearchService.
//   - HttpClient duoc inject qua HttpClientFactory tu DependencyInjection.
//   - Endpoint mac dinh: http://localhost:6333.
// Luu y:
//   - Qdrant point ID phai la unsigned integer hoac UUID. Caller dung string nhu
//     "knowledge_<docId>_<idx>" -> hash thanh UUID v5 de Qdrant chap nhan.
// =============================================================================
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.Infrastructure.Services;

public class QdrantVectorSearchService : IVectorSearchService
{
    private readonly HttpClient _http;
    private readonly QdrantSettings _settings;

    public QdrantVectorSearchService(
        HttpClient http,
        IOptions<QdrantSettings> settings)
    {
        _http = http;
        _settings = settings.Value;
    }

    public async Task EnsureCollectionAsync(CancellationToken ct = default)
    {
        var url = $"{_settings.BaseUrl.TrimEnd('/')}/collections/{_settings.CollectionName}";

        // Kiem tra ton tai chua: GET tra ve 200 neu da co.
        using (var probe = await _http.GetAsync(url, ct))
        {
            if (probe.IsSuccessStatusCode)
                return;
        }

        var body = new
        {
            vectors = new
            {
                size = _settings.VectorSize,
                distance = "Cosine"
            }
        };

        using var response = await _http.PutAsJsonAsync(url, body, ct);

        if (!response.IsSuccessStatusCode)
        {
            var raw = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Qdrant create collection loi {(int)response.StatusCode}: {raw}");
        }
    }

    public async Task UpsertAsync(
        string vectorId,
        float[] vector,
        Dictionary<string, object?> payload,
        CancellationToken ct = default)
    {
        var url = $"{_settings.BaseUrl.TrimEnd('/')}/collections/{_settings.CollectionName}/points?wait=true";

        // Luu lai logical id trong payload de search co the map nguoc.
        payload["vectorId"] = vectorId;

        var body = new
        {
            points = new[]
            {
                new
                {
                    id = ToQdrantId(vectorId),
                    vector,
                    payload
                }
            }
        };

        using var response = await _http.PutAsJsonAsync(url, body, ct);

        if (!response.IsSuccessStatusCode)
        {
            var raw = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Qdrant upsert loi {(int)response.StatusCode}: {raw}");
        }
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        float[] queryVector,
        int topK = 5,
        CancellationToken ct = default)
    {
        var url = $"{_settings.BaseUrl.TrimEnd('/')}/collections/{_settings.CollectionName}/points/search";

        var body = new
        {
            vector = queryVector,
            limit = topK,
            with_payload = true
        };

        using var response = await _http.PostAsJsonAsync(url, body, ct);
        var raw = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Qdrant search loi {(int)response.StatusCode}: {raw}");

        using var doc = JsonDocument.Parse(raw);

        var results = new List<VectorSearchResult>();

        foreach (var item in doc.RootElement.GetProperty("result").EnumerateArray())
        {
            var result = new VectorSearchResult
            {
                Score = item.GetProperty("score").GetDecimal()
            };

            if (item.TryGetProperty("payload", out var payload))
            {
                foreach (var prop in payload.EnumerateObject())
                {
                    result.Payload[prop.Name] = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => prop.Value.GetString(),
                        JsonValueKind.Number => prop.Value.TryGetInt64(out var l) ? l : prop.Value.GetDecimal(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => prop.Value.ToString()
                    };
                }

                if (result.Payload.TryGetValue("vectorId", out var vid) && vid is string s)
                    result.VectorId = s;
            }

            if (string.IsNullOrEmpty(result.VectorId))
                result.VectorId = item.GetProperty("id").ToString();

            results.Add(result);
        }

        return results;
    }

    // Bien chuoi vectorId thanh UUID deterministic (16 bytes -> UUID). Qdrant chi
    // nhan integer hoac UUID lam point ID, nen ta hash SHA-256 va lay 16 byte dau.
    private static string ToQdrantId(string vectorId)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(vectorId));
        var guidBytes = new byte[16];
        Array.Copy(bytes, guidBytes, 16);
        return new Guid(guidBytes).ToString();
    }
}
