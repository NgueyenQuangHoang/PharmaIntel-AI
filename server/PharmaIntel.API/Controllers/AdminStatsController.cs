// =============================================================================
// Controller: AdminStatsController
// Chuc nang: Cung cap so lieu thong ke cho admin dashboard (read-only).
// Endpoints (yeu cau JWT + role=admin):
//   GET /api/admin/stats/overview              card tong quan
//   GET /api/admin/stats/revenue?range=7d|30d|90d   time series doanh thu
//   GET /api/admin/stats/top-medications?limit=10   top SP ban chay
//   GET /api/admin/stats/orders-by-status      pie chart trang thai don
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.Core.DTOs.Admin;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize(Roles = "admin")]
[Route("api/admin/stats")]
public class AdminStatsController : ControllerBase
{
    private readonly IAdminStatsService _service;

    public AdminStatsController(IAdminStatsService service)
    {
        _service = service;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<AdminStatsOverviewDto>> Overview(CancellationToken ct)
        => Ok(await _service.GetOverviewAsync(ct));

    [HttpGet("revenue")]
    public async Task<ActionResult<List<RevenuePointDto>>> Revenue(
        [FromQuery] string range = "7d", CancellationToken ct = default)
    {
        var days = ParseRange(range);
        return Ok(await _service.GetRevenueAsync(days, ct));
    }

    [HttpGet("top-medications")]
    public async Task<ActionResult<List<TopMedicationDto>>> TopMedications(
        [FromQuery] int limit = 10, CancellationToken ct = default)
        => Ok(await _service.GetTopMedicationsAsync(limit, ct));

    [HttpGet("orders-by-status")]
    public async Task<ActionResult<List<OrdersByStatusDto>>> OrdersByStatus(CancellationToken ct)
        => Ok(await _service.GetOrdersByStatusAsync(ct));

    private static int ParseRange(string range)
    {
        return (range ?? "").Trim().ToLowerInvariant() switch
        {
            "7d" => 7,
            "30d" => 30,
            "90d" => 90,
            _ => 7
        };
    }
}
