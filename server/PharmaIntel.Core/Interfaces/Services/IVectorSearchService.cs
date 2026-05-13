// =============================================================================
// Interface: IVectorSearchService
// Chuc nang: Truu tuong hoa vector DB (Qdrant la implementation hien tai).
//            Cho phep swap sang Azure AI Search/pgvector sau nay khong sua caller.
// =============================================================================
namespace PharmaIntel.Core.Interfaces.Services;

public interface IVectorSearchService
{
    Task EnsureCollectionAsync(CancellationToken ct = default);

    Task UpsertAsync(
        string vectorId,
        float[] vector,
        Dictionary<string, object?> payload,
        CancellationToken ct = default);

    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        float[] queryVector,
        int topK = 5,
        CancellationToken ct = default);
}

public class VectorSearchResult
{
    public string VectorId { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public Dictionary<string, object?> Payload { get; set; } = new();
}
