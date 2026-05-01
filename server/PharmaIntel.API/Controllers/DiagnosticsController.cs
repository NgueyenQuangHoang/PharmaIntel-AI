// =============================================================================
// Controller: DiagnosticsController
// Chuc nang: Phien chan doan AI cua user (user-scoped, yeu cau JWT).
// Endpoints:
//   POST /api/diagnostics/sessions                tao phien moi (chon trieu chung)
//   GET  /api/diagnostics/sessions/my             list phien cua minh (paged + filter status)
//   GET  /api/diagnostics/sessions/{id}           chi tiet kem messages + result
//   POST /api/diagnostics/sessions/{id}/messages  user gui tin nhan (AI tu dong reply)
//   POST /api/diagnostics/sessions/{id}/complete  ket thuc + chay AI engine + tao result
//   GET  /api/diagnostics/results/{id}            chi tiet ket qua + thuoc de xuat
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Diagnostics;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize]
[Route("api/diagnostics")]
public class DiagnosticsController : ControllerBase
{
    private readonly IDiagnosticService _service;

    public DiagnosticsController(IDiagnosticService service)
    {
        _service = service;
    }

    [HttpPost("sessions")]
    public async Task<ActionResult<DiagnosticSessionDto>> CreateSession(
        [FromBody] CreateDiagnosticSessionRequest request, CancellationToken ct)
    {
        var created = await _service.CreateSessionAsync(User.GetUserId(), request, ct);
        return CreatedAtAction(nameof(GetSession), new { id = created.Id }, created);
    }

    [HttpGet("sessions/my")]
    public async Task<ActionResult<PagedResult<DiagnosticSessionListItemDto>>> ListMySessions(
        [FromQuery] DiagnosticSessionListQuery query, CancellationToken ct)
        => Ok(await _service.ListMySessionsAsync(User.GetUserId(), query, ct));

    [HttpGet("sessions/{id:long}")]
    public async Task<ActionResult<DiagnosticSessionDto>> GetSession(long id, CancellationToken ct)
        => Ok(await _service.GetSessionByIdAsync(User.GetUserId(), id, ct));

    [HttpPost("sessions/{id:long}/messages")]
    public async Task<ActionResult<DiagnosticMessageDto>> AddMessage(
        long id, [FromBody] AddDiagnosticMessageRequest request, CancellationToken ct)
    {
        var msg = await _service.AddMessageAsync(User.GetUserId(), id, request, ct);
        return CreatedAtAction(nameof(GetSession), new { id }, msg);
    }

    [HttpPost("sessions/{id:long}/complete")]
    public async Task<ActionResult<DiagnosticSessionDto>> Complete(long id, CancellationToken ct)
        => Ok(await _service.CompleteSessionAsync(User.GetUserId(), id, ct));

    [HttpGet("results/{id:long}")]
    public async Task<ActionResult<DiagnosticResultDto>> GetResult(long id, CancellationToken ct)
        => Ok(await _service.GetResultByIdAsync(User.GetUserId(), id, ct));
}
