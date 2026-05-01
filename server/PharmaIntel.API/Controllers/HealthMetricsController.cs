// =============================================================================
// Controller: HealthMetricsController
// Chuc nang: CRUD chi so suc khoe cua user dang dang nhap (user-scoped).
// Endpoints (tat ca yeu cau JWT):
//   GET    /api/health-metrics             list (paged + filter type/date range)
//   GET    /api/health-metrics/{id}        chi tiet
//   POST   /api/health-metrics             ghi chi so moi
//   PUT    /api/health-metrics/{id}        cap nhat
//   DELETE /api/health-metrics/{id}        xoa
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.HealthMetrics;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize]
[Route("api/health-metrics")]
public class HealthMetricsController : ControllerBase
{
    private readonly IHealthMetricService _service;

    public HealthMetricsController(IHealthMetricService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<HealthMetricDto>>> List(
        [FromQuery] HealthMetricListQuery query, CancellationToken ct)
        => Ok(await _service.ListMyAsync(User.GetUserId(), query, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<HealthMetricDto>> Get(long id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(User.GetUserId(), id, ct));

    [HttpPost]
    public async Task<ActionResult<HealthMetricDto>> Create(
        [FromBody] HealthMetricCreateRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(User.GetUserId(), request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<HealthMetricDto>> Update(
        long id, [FromBody] HealthMetricUpdateRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(User.GetUserId(), id, request, ct));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(User.GetUserId(), id, ct);
        return NoContent();
    }
}
