// Service: AdminUserService
// Chuc nang: Quan ly user cho admin (list, doi role, lock/unlock, xoa mem).
// Quy tac:
//   - Khong cho admin doi role / lock / xoa chinh ban than -> ConflictException
//   - Role chi nhan "user" hoac "admin" (lowercase) -> ValidationException
//   - DeleteAsync = soft delete + anonymize PII (KHONG hard delete):
//       * IsActive = false -> AuthService.LoginAsync chan login
//       * Email/FullName/AvatarUrl/AuthProviderId duoc anonymize
//       * PasswordHash xoa de chan login local
//       * Giu lai user record + cac FK orders/payments/prescriptions/audit
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Admin;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class AdminUserService : IAdminUserService
{
    private static readonly string[] AllowedRoles = ["user", "admin"];

    private readonly PharmaIntelDbContext _db;

    public AdminUserService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<AdminUserDto>> ListAsync(AdminUserListQuery q, CancellationToken ct = default)
    {
        q.Normalize();

        var query = _db.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Q))
        {
            var k = q.Q.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(k) || u.Email.ToLower().Contains(k));
        }

        if (!string.IsNullOrWhiteSpace(q.Role))
        {
            var role = q.Role.Trim().ToLower();
            query = query.Where(u => u.Role == role);
        }

        if (q.IsActive.HasValue)
            query = query.Where(u => u.IsActive == q.IsActive);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                AvatarUrl = u.AvatarUrl,
                Role = u.Role,
                IsActive = u.IsActive,
                AuthProvider = u.AuthProvider,
                TotalOrders = u.Orders.Count,
                TotalSpent = u.Orders
                    .Where(o => o.Status == "delivered" || o.PaymentStatus == "paid")
                    .Sum(o => (decimal?)o.Total) ?? 0m,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<AdminUserDto>
        {
            Items = items,
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    public async Task<AdminUserDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var dto = await _db.Users.AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                AvatarUrl = u.AvatarUrl,
                Role = u.Role,
                IsActive = u.IsActive,
                AuthProvider = u.AuthProvider,
                TotalOrders = u.Orders.Count,
                TotalSpent = u.Orders
                    .Where(o => o.Status == "delivered" || o.PaymentStatus == "paid")
                    .Sum(o => (decimal?)o.Total) ?? 0m,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        return dto ?? throw new NotFoundException("nguoi dung", id);
    }

    public async Task<AdminUserDto> UpdateRoleAsync(long currentUserId, long targetUserId, UpdateUserRoleRequest req, CancellationToken ct = default)
    {
        if (currentUserId == targetUserId)
            throw new ConflictException("Khong the doi role cua chinh ban than");

        var role = (req.Role ?? "").Trim().ToLowerInvariant();
        if (!AllowedRoles.Contains(role))
            throw new ValidationException("role", $"Role khong hop le. Chi chap nhan: {string.Join(", ", AllowedRoles)}");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == targetUserId, ct)
                   ?? throw new NotFoundException("nguoi dung", targetUserId);

        user.Role = role;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(targetUserId, ct);
    }

    public async Task<AdminUserDto> UpdateStatusAsync(long currentUserId, long targetUserId, UpdateUserStatusRequest req, CancellationToken ct = default)
    {
        if (currentUserId == targetUserId)
            throw new ConflictException("Khong the lock/unlock chinh ban than");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == targetUserId, ct)
                   ?? throw new NotFoundException("nguoi dung", targetUserId);

        user.IsActive = req.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(targetUserId, ct);
    }

    public async Task DeleteAsync(long currentUserId, long targetUserId, CancellationToken ct = default)
    {
        if (currentUserId == targetUserId)
            throw new ConflictException("Khong the xoa chinh ban than");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == targetUserId, ct)
                   ?? throw new NotFoundException("nguoi dung", targetUserId);

        // Soft delete + anonymize PII (GDPR-friendly).
        // Giu lai user record de orders/payments/prescriptions/audit van con FK hop le.
        // Khong the undo - email goc se khong phuc hoi duoc.
        if (!user.IsActive && user.Email.StartsWith("deleted-"))
            throw new ConflictException("User da bi xoa truoc do");

        user.IsActive = false;
        user.Email = $"deleted-{user.Id}@anonymized.local";
        user.FullName = "[Deleted User]";
        user.PasswordHash = null;        // chan login local
        user.AvatarUrl = null;
        user.AuthProviderId = null;      // chan re-login OAuth
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }
}
