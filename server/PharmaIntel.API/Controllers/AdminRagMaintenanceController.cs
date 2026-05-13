// =============================================================================
// Controller: AdminRagMaintenanceController (Phase 5)
// Endpoints (admin only):
//   POST /api/admin/rag-maintenance/check-consistency?autoReindex=true
//   POST /api/admin/rag-maintenance/documents/{id}/enqueue-reindex
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize(Roles = "admin")]
[Route("api/admin/rag-maintenance")]
public class AdminRagMaintenanceController : ControllerBase
{
    private readonly IRagMaintenanceService _maintenance;

    public AdminRagMaintenanceController(IRagMaintenanceService maintenance)
    {
        _maintenance = maintenance;
    }

    [HttpPost("check-consistency")]
    public async Task<ActionResult<ConsistencyCheckResult>> CheckConsistency(
        [FromQuery] bool autoReindex = false,
        CancellationToken ct = default)
    {
        return Ok(await _maintenance.CheckConsistencyAsync(autoReindex, ct));
    }

    [HttpPost("documents/{id:long}/enqueue-reindex")]
    public async Task<ActionResult<object>> EnqueueReindex(long id, CancellationToken ct)
    {
        var jobId = await _maintenance.EnqueueReindexAsync(id, ct);
        return Ok(new { jobId });
    }
}
