// =============================================================================
// Controller: AdminRagDashboardController
// Chuc nang: Endpoint admin lay metric tong hop chat luong RAG (Phase 4).
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.Core.DTOs.RagDashboard;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Route("api/admin/rag-dashboard")]
[Authorize(Roles = "admin")]
public class AdminRagDashboardController : ControllerBase
{
    private readonly IRagDashboardService _dashboard;

    public AdminRagDashboardController(IRagDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    [HttpGet]
    public async Task<ActionResult<RagDashboardDto>> Get(CancellationToken ct)
    {
        return Ok(await _dashboard.GetAsync(ct));
    }
}
