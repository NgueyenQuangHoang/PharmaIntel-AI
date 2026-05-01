// =============================================================================
// Controller: DbCheckController
// Chuc nang: Endpoint kiem tra ket noi database, dem so bang va so migration.
// Endpoint: GET /api/db-check
// =============================================================================
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Route("api/db-check")]
public class DbCheckController : ControllerBase
{
    private readonly PharmaIntelDbContext _context;

    public DbCheckController(PharmaIntelDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                return StatusCode(503, new
                {
                    status = "error",
                    database = "disconnected",
                    message = "Khong the ket noi den database"
                });
            }

            var conn = _context.Database.GetDbConnection();
            var tableCount = 0;
            var tables = new List<string>();
            await conn.OpenAsync();
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
                                    WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME <> '__EFMigrationsHistory'
                                    ORDER BY TABLE_NAME";
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                    tableCount++;
                }
            }
            finally
            {
                await conn.CloseAsync();
            }

            var appliedMigrations = (await _context.Database.GetAppliedMigrationsAsync()).ToList();
            var pendingMigrations = (await _context.Database.GetPendingMigrationsAsync()).ToList();

            return Ok(new
            {
                status = "ok",
                database = "connected",
                provider = _context.Database.ProviderName,
                databaseName = conn.Database,
                tableCount,
                tables,
                appliedMigrations,
                pendingMigrations,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "error",
                message = ex.Message,
                type = ex.GetType().Name
            });
        }
    }
}
