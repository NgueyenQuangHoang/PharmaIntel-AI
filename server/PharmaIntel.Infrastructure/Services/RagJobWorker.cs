// =============================================================================
// Service: RagJobWorker (BackgroundService)
// Chuc nang: Poll bang rag_jobs moi 5s, lay 1 job queued cu nhat, chay theo
//            JobType, cap nhat status. Phuc vu ingest/reindex/delete_vector
//            bat dong bo (Phase 5).
// Quan he:
//   - HostedService singleton.
//   - Dung IServiceScopeFactory de tao scope cho DbContext + service moi job.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class RagJobWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RagJobWorker> _logger;

    public RagJobWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<RagJobWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RagJobWorker started, poll interval {Interval}s.", PollInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOneJobAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RAG job worker failed unexpectedly.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private async Task ProcessOneJobAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PharmaIntelDbContext>();

        var job = await db.RagJobs
            .Where(x => x.Status == "queued")
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (job == null)
            return;

        job.Status = "running";
        job.StartedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        try
        {
            var ingestion = scope.ServiceProvider.GetRequiredService<IKnowledgeIngestionService>();

            switch (job.JobType)
            {
                case "reindex" when job.DocumentId.HasValue:
                    await ingestion.ReindexAsync(job.DocumentId.Value, ct);
                    break;

                case "delete_vector" when job.DocumentId.HasValue:
                    await ingestion.DeleteAsync(job.DocumentId.Value, ct);
                    break;

                case "ingest":
                    // Payload-driven ingest se duoc them sau khi co schema PayloadJson chuan.
                    _logger.LogWarning("Job {Id} type=ingest chua co handler", job.Id);
                    break;

                default:
                    throw new InvalidOperationException($"JobType '{job.JobType}' khong duoc ho tro hoac thieu DocumentId.");
            }

            job.Status = "completed";
            job.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} ({JobType}) failed", job.Id, job.JobType);
            job.Status = "failed";
            job.ErrorMessage = ex.Message.Length > 3900 ? ex.Message[..3900] : ex.Message;
            job.CompletedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
    }
}
