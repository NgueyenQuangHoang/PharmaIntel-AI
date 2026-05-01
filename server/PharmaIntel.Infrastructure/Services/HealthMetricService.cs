// =============================================================================
// Service: HealthMetricService
// Chuc nang: CRUD chi so suc khoe (user-scoped).
// Quan he: N:1 voi User (cascade delete khi user bi xoa).
// Quy tac:
//   - Filter mac dinh theo userId; truy cap chi so cua user khac -> 403.
//   - Auto-fill `Unit` mac dinh theo MetricType neu user khong nhap.
//   - `RecordedAt` mac dinh = UtcNow neu null. Validator chan gia tri tuong lai.
//   - Index (UserId, MetricType, RecordedAt) toi uu cho list+filter+chart.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.HealthMetrics;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class HealthMetricService : IHealthMetricService
{
    private readonly PharmaIntelDbContext _db;

    public HealthMetricService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<HealthMetricDto>> ListMyAsync(long userId, HealthMetricListQuery q, CancellationToken ct = default)
    {
        q.Normalize();

        var query = _db.HealthMetrics.AsNoTracking().Where(m => m.UserId == userId);

        if (!string.IsNullOrWhiteSpace(q.MetricType))
            query = query.Where(m => m.MetricType == q.MetricType);
        if (q.FromDate.HasValue)
            query = query.Where(m => m.RecordedAt >= q.FromDate.Value);
        if (q.ToDate.HasValue)
            query = query.Where(m => m.RecordedAt <= q.ToDate.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(m => m.RecordedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(m => new HealthMetricDto
            {
                Id = m.Id,
                MetricType = m.MetricType,
                ValueNumber = m.ValueNumber,
                ValueNumber2 = m.ValueNumber2,
                Unit = m.Unit,
                Notes = m.Notes,
                RecordedAt = m.RecordedAt
            })
            .ToListAsync(ct);

        return new PagedResult<HealthMetricDto>
        {
            Items = items,
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    public async Task<HealthMetricDto> GetByIdAsync(long userId, long id, CancellationToken ct = default)
    {
        var m = await _db.HealthMetrics.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new NotFoundException("chi so suc khoe", id);
        if (m.UserId != userId)
            throw new ForbiddenException("Chi so khong thuoc ve ban");
        return ToDto(m);
    }

    public async Task<HealthMetricDto> CreateAsync(long userId, HealthMetricCreateRequest req, CancellationToken ct = default)
    {
        var entity = new HealthMetric
        {
            UserId = userId,
            MetricType = req.MetricType,
            ValueNumber = req.ValueNumber,
            ValueNumber2 = req.MetricType == "blood_pressure" ? req.ValueNumber2 : null,
            Unit = string.IsNullOrWhiteSpace(req.Unit) ? DefaultUnit(req.MetricType) : req.Unit.Trim(),
            Notes = string.IsNullOrWhiteSpace(req.Notes) ? null : req.Notes.Trim(),
            RecordedAt = req.RecordedAt ?? DateTime.UtcNow
        };

        _db.HealthMetrics.Add(entity);
        await _db.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<HealthMetricDto> UpdateAsync(long userId, long id, HealthMetricUpdateRequest req, CancellationToken ct = default)
    {
        var entity = await _db.HealthMetrics.FirstOrDefaultAsync(m => m.Id == id, ct)
                     ?? throw new NotFoundException("chi so suc khoe", id);
        if (entity.UserId != userId)
            throw new ForbiddenException("Chi so khong thuoc ve ban");

        entity.MetricType = req.MetricType;
        entity.ValueNumber = req.ValueNumber;
        entity.ValueNumber2 = req.MetricType == "blood_pressure" ? req.ValueNumber2 : null;
        entity.Unit = string.IsNullOrWhiteSpace(req.Unit) ? DefaultUnit(req.MetricType) : req.Unit.Trim();
        entity.Notes = string.IsNullOrWhiteSpace(req.Notes) ? null : req.Notes.Trim();
        entity.RecordedAt = req.RecordedAt ?? entity.RecordedAt;

        await _db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task DeleteAsync(long userId, long id, CancellationToken ct = default)
    {
        var entity = await _db.HealthMetrics.FirstOrDefaultAsync(m => m.Id == id, ct)
                     ?? throw new NotFoundException("chi so suc khoe", id);
        if (entity.UserId != userId)
            throw new ForbiddenException("Chi so khong thuoc ve ban");

        _db.HealthMetrics.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    private static string DefaultUnit(string metricType) => metricType switch
    {
        "blood_pressure"    => "mmHg",
        "heart_rate"        => "bpm",
        "temperature"       => "C",
        "weight"            => "kg",
        "blood_sugar"       => "mmol/L",
        "oxygen_saturation" => "%",
        _ => string.Empty
    };

    private static HealthMetricDto ToDto(HealthMetric m) => new()
    {
        Id = m.Id,
        MetricType = m.MetricType,
        ValueNumber = m.ValueNumber,
        ValueNumber2 = m.ValueNumber2,
        Unit = m.Unit,
        Notes = m.Notes,
        RecordedAt = m.RecordedAt
    };
}
