// =============================================================================
// Controller: HealthController
// Chuc nang: Endpoint kiem tra trang thai hoat dong cua API va ket noi database.
// Endpoint: GET /api/health
// =============================================================================
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly PharmaIntelDbContext _context;

    public HealthController(PharmaIntelDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var canConnect = false;
        try
        {
            canConnect = await _context.Database.CanConnectAsync();
        }
        catch
        {
            // Database chua san sang - van tra ve API status
        }

        return Ok(new
        {
            status = "running",
            database = canConnect ? "connected" : "disconnected",
            timestamp = DateTime.UtcNow
        });
    }
}
