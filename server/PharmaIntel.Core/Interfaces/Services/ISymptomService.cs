// =============================================================================
// Interface: ISymptomService
// Chuc nang: Doc danh muc trieu chung (public catalog).
// =============================================================================
using PharmaIntel.Core.DTOs.Diagnostics;

namespace PharmaIntel.Core.Interfaces.Services;

public interface ISymptomService
{
    Task<List<SymptomDto>> ListAsync(string? groupName, CancellationToken ct = default);
}
