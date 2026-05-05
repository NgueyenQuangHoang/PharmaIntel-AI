// =============================================================================
// Service: AdminStatsService
// Chuc nang: Tinh thong ke cho admin dashboard.
// Nguyen tac:
//   - Doanh thu tinh tu Order.Total cho cac don da paid (paymentStatus = "paid")
//     hoac da delivered, KHONG tinh don cancelled/refunded
//   - "Today" duoc xac dinh theo UTC (vi DB luu DATETIME2 UTC); FE co the convert
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Admin;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class AdminStatsService : IAdminStatsService
{
    private readonly PharmaIntelDbContext _db;

    public AdminStatsService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<AdminStatsOverviewDto> GetOverviewAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var totalUsers = await _db.Users.CountAsync(ct);
        var totalAdmins = await _db.Users.CountAsync(u => u.Role == "admin", ct);
        var activeUsers = await _db.Users.CountAsync(u => u.IsActive, ct);
        var totalOrders = await _db.Orders.CountAsync(ct);
        var totalRevenue = await _db.Orders
            .Where(o => o.Status != "cancelled" && o.Status != "refunded"
                        && (o.PaymentStatus == "paid" || o.Status == "delivered"))
            .SumAsync(o => (decimal?)o.Total, ct) ?? 0m;
        var totalMedications = await _db.Medications.CountAsync(ct);
        var totalCategories = await _db.Categories.CountAsync(ct);

        var ordersToday = await _db.Orders
            .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow)
            .CountAsync(ct);
        var revenueToday = await _db.Orders
            .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow
                        && o.Status != "cancelled" && o.Status != "refunded"
                        && (o.PaymentStatus == "paid" || o.Status == "delivered"))
            .SumAsync(o => (decimal?)o.Total, ct) ?? 0m;
        var ordersPending = await _db.Orders
            .Where(o => o.Status == "pending" || o.Status == "confirmed" || o.Status == "processing")
            .CountAsync(ct);

        return new AdminStatsOverviewDto
        {
            TotalUsers = totalUsers,
            TotalAdmins = totalAdmins,
            ActiveUsers = activeUsers,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            TotalMedications = totalMedications,
            TotalCategories = totalCategories,
            OrdersToday = ordersToday,
            RevenueToday = revenueToday,
            OrdersPending = ordersPending
        };
    }

    public async Task<List<RevenuePointDto>> GetRevenueAsync(int days, CancellationToken ct = default)
    {
        if (days < 1) days = 7;
        if (days > 365) days = 365;

        var endExclusive = DateTime.UtcNow.Date.AddDays(1);
        var start = endExclusive.AddDays(-days);

        var raw = await _db.Orders
            .Where(o => o.CreatedAt >= start && o.CreatedAt < endExclusive
                        && o.Status != "cancelled" && o.Status != "refunded"
                        && (o.PaymentStatus == "paid" || o.Status == "delivered"))
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Revenue = g.Sum(x => x.Total),
                Count = g.Count()
            })
            .ToListAsync(ct);

        var byDate = raw.ToDictionary(x => x.Date, x => x);
        var result = new List<RevenuePointDto>(days);
        for (int i = 0; i < days; i++)
        {
            var d = start.AddDays(i);
            byDate.TryGetValue(d, out var v);
            result.Add(new RevenuePointDto
            {
                Date = DateOnly.FromDateTime(d),
                Revenue = v?.Revenue ?? 0m,
                OrderCount = v?.Count ?? 0
            });
        }
        return result;
    }

    public async Task<List<TopMedicationDto>> GetTopMedicationsAsync(int limit, CancellationToken ct = default)
    {
        if (limit < 1) limit = 10;
        if (limit > 50) limit = 50;

        return await _db.OrderItems
            .Where(oi => oi.MedicationId != null && oi.Medication != null
                         && oi.Order.Status != "cancelled" && oi.Order.Status != "refunded")
            .GroupBy(oi => new { MedicationId = oi.MedicationId!.Value, oi.Medication!.Name, oi.Medication.ImageUrl })
            .Select(g => new TopMedicationDto
            {
                MedicationId = g.Key.MedicationId,
                Name = g.Key.Name,
                ImageUrl = g.Key.ImageUrl,
                QuantitySold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<List<OrdersByStatusDto>> GetOrdersByStatusAsync(CancellationToken ct = default)
    {
        return await _db.Orders
            .GroupBy(o => o.Status)
            .Select(g => new OrdersByStatusDto
            {
                Status = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(ct);
    }
}
