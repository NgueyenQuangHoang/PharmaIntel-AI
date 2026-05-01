// =============================================================================
// Extension: AppBootstrapExtensions
// Chuc nang: Auto-migrate DB + seed du lieu mau luc startup, dieu khien qua config:
//   "Bootstrap": {
//     "MigrateOnStartup": true,
//     "Seed": { "Enabled": true }
//   }
// Production mac dinh tat 2 co - admin chu dong chay `dotnet ef database update`.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Infrastructure.Data;
using PharmaIntel.Infrastructure.Data.Seeders;

namespace PharmaIntel.API.Extensions;

public static class AppBootstrapExtensions
{
    public static async Task<WebApplication> MigrateAndSeedAsync(this WebApplication app, CancellationToken ct = default)
    {
        var config = app.Configuration.GetSection("Bootstrap");
        var migrateEnabled = config.GetValue("MigrateOnStartup", false);
        var seedEnabled = config.GetSection("Seed").GetValue("Enabled", false);

        if (!migrateEnabled && !seedEnabled)
        {
            app.Logger.LogInformation("Bootstrap: migrate va seed deu tat - bo qua.");
            return app;
        }

        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("Bootstrap");

        try
        {
            if (migrateEnabled)
            {
                logger.LogInformation("Bootstrap: applying migrations...");
                var db = sp.GetRequiredService<PharmaIntelDbContext>();
                await db.Database.MigrateAsync(ct);
                logger.LogInformation("Bootstrap: migrations applied.");
            }

            if (seedEnabled)
            {
                logger.LogInformation("Bootstrap: seeding initial data...");
                var seeder = sp.GetRequiredService<DataSeeder>();
                await seeder.SeedAsync(ct);
                logger.LogInformation("Bootstrap: seed complete.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Bootstrap: migrate/seed FAILED.");
            throw;
        }

        return app;
    }
}
