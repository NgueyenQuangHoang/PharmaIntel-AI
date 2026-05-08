// =============================================================================
// Service: AddressService
// Chuc nang: Quan ly dia chi giao hang cua user (user-scoped CRUD).
// Quan he: N:1 voi User | duoc tham chieu boi Order (snapshot khi checkout)
// Quy tac:
//   - Moi user chi co 1 dia chi default (UX_addresses_user_default)
//   - Dia chi dau tien tao ra mac dinh la default
//   - Khong xoa cung neu da co order tham chieu -> soft delete (IsActive=false)
//   - Khi xoa default -> tu dong promote dia chi moi nhat lam default
//   - Moi thao tac filter theo userId; truy cap dia chi nguoi khac -> Forbidden
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Addresses;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class AddressService : IAddressService
{
    private readonly PharmaIntelDbContext _db;

    public AddressService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<AddressDto>> ListMyAsync(long userId, AddressListQuery q, CancellationToken ct = default)
    {
        q.Normalize();

        var query = _db.Addresses.AsNoTracking().Where(a => a.UserId == userId);

        if (!string.IsNullOrWhiteSpace(q.Q))
        {
            var k = q.Q.Trim().ToLower();
            query = query.Where(a => a.RecipientName.ToLower().Contains(k)
                                  || a.StreetAddress.ToLower().Contains(k));
        }
        if (q.IsActive.HasValue) query = query.Where(a => a.IsActive == q.IsActive);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(a => new AddressDto
            {
                Id = a.Id,
                UserId = a.UserId,
                RecipientName = a.RecipientName,
                Phone = a.Phone,
                Province = a.Province,
                District = a.District,
                Ward = a.Ward,
                StreetAddress = a.StreetAddress,
                FullAddress = string.IsNullOrWhiteSpace(a.District) 
                    ? a.StreetAddress + ", " + a.Ward + ", " + a.Province
                    : a.StreetAddress + ", " + a.Ward + ", " + a.District + ", " + a.Province,
                IsDefault = a.IsDefault,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<AddressDto>
        {
            Items = items,
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    public async Task<AddressDto> GetByIdAsync(long userId, long id, CancellationToken ct = default)
    {
        var addr = await _db.Addresses.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct)
                   ?? throw new NotFoundException("dia chi", id);
        if (addr.UserId != userId)
            throw new ForbiddenException("Dia chi khong thuoc ve ban");
        return ToDto(addr);
    }

    public async Task<AddressDto> CreateAsync(long userId, AddressCreateRequest req, CancellationToken ct = default)
    {
        var hasAny = await _db.Addresses.AnyAsync(a => a.UserId == userId, ct);
        var shouldBeDefault = req.IsDefault || !hasAny;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        if (shouldBeDefault && hasAny)
        {
            await _db.Addresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false), ct);
        }

        var entity = new Address
        {
            UserId = userId,
            RecipientName = req.RecipientName.Trim(),
            Phone = req.Phone.Trim(),
            Province = req.Province.Trim(),
            District = req.District.Trim(),
            Ward = req.Ward.Trim(),
            StreetAddress = req.StreetAddress.Trim(),
            IsDefault = shouldBeDefault,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Addresses.Add(entity);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return ToDto(entity);
    }

    public async Task<AddressDto> UpdateAsync(long userId, long id, AddressUpdateRequest req, CancellationToken ct = default)
    {
        var entity = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id, ct)
                     ?? throw new NotFoundException("dia chi", id);
        if (entity.UserId != userId)
            throw new ForbiddenException("Dia chi khong thuoc ve ban");

        entity.RecipientName = req.RecipientName.Trim();
        entity.Phone = req.Phone.Trim();
        entity.Province = req.Province.Trim();
        entity.District = req.District.Trim();
        entity.Ward = req.Ward.Trim();
        entity.StreetAddress = req.StreetAddress.Trim();
        entity.IsActive = req.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task DeleteAsync(long userId, long id, CancellationToken ct = default)
    {
        var entity = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id, ct)
                     ?? throw new NotFoundException("dia chi", id);
        if (entity.UserId != userId)
            throw new ForbiddenException("Dia chi khong thuoc ve ban");

        var hasOrders = await _db.Orders.AnyAsync(o => o.AddressId == id, ct);

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var wasDefault = entity.IsDefault;

        if (hasOrders)
        {
            // Soft delete: dia chi da duoc dung trong don hang -> giu lai cho lich su
            entity.IsActive = false;
            entity.IsDefault = false;
            entity.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _db.Addresses.Remove(entity);
        }

        await _db.SaveChangesAsync(ct);

        // Promote dia chi moi nhat con active lam default neu can
        if (wasDefault)
        {
            var candidate = await _db.Addresses
                .Where(a => a.UserId == userId && a.IsActive && a.Id != id)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(ct);
            if (candidate != null)
            {
                candidate.IsDefault = true;
                candidate.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
            }
        }

        await tx.CommitAsync(ct);
    }

    public async Task<AddressDto> SetDefaultAsync(long userId, long id, CancellationToken ct = default)
    {
        var entity = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id, ct)
                     ?? throw new NotFoundException("dia chi", id);
        if (entity.UserId != userId)
            throw new ForbiddenException("Dia chi khong thuoc ve ban");
        if (!entity.IsActive)
            throw new ConflictException("Dia chi khong con hoat dong, khong the dat lam mac dinh");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        await _db.Addresses
            .Where(a => a.UserId == userId && a.IsDefault && a.Id != id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false), ct);

        if (!entity.IsDefault)
        {
            entity.IsDefault = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        await tx.CommitAsync(ct);
        return ToDto(entity);
    }

    private static AddressDto ToDto(Address a) => new()
    {
        Id = a.Id,
        UserId = a.UserId,
        RecipientName = a.RecipientName,
        Phone = a.Phone,
        Province = a.Province,
        District = a.District,
        Ward = a.Ward,
        StreetAddress = a.StreetAddress,
        FullAddress = string.IsNullOrWhiteSpace(a.District) 
            ? $"{a.StreetAddress}, {a.Ward}, {a.Province}"
            : $"{a.StreetAddress}, {a.Ward}, {a.District}, {a.Province}",
        IsDefault = a.IsDefault,
        IsActive = a.IsActive,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}
