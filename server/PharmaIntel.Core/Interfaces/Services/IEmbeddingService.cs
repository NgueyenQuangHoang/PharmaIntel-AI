// =============================================================================
// Interface: IEmbeddingService
// Chuc nang: Sinh embedding vector cho 1 doan text (dung cho RAG Phase 2).
// =============================================================================
namespace PharmaIntel.Core.Interfaces.Services;

public interface IEmbeddingService
{
    Task<float[]> EmbedAsync(string text, CancellationToken ct = default);
}
