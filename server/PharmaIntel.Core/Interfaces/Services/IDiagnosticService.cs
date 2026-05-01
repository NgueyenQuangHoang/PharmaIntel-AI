// =============================================================================
// Interface: IDiagnosticService
// Chuc nang: Quan ly phien chan doan AI (user-scoped) + complete flow.
// =============================================================================
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Diagnostics;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IDiagnosticService
{
    Task<DiagnosticSessionDto> CreateSessionAsync(long userId, CreateDiagnosticSessionRequest request, CancellationToken ct = default);
    Task<PagedResult<DiagnosticSessionListItemDto>> ListMySessionsAsync(long userId, DiagnosticSessionListQuery query, CancellationToken ct = default);
    Task<DiagnosticSessionDto> GetSessionByIdAsync(long userId, long sessionId, CancellationToken ct = default);
    Task<DiagnosticMessageDto> AddMessageAsync(long userId, long sessionId, AddDiagnosticMessageRequest request, CancellationToken ct = default);
    Task<DiagnosticSessionDto> CompleteSessionAsync(long userId, long sessionId, CancellationToken ct = default);
    Task<DiagnosticResultDto> GetResultByIdAsync(long userId, long resultId, CancellationToken ct = default);
}
