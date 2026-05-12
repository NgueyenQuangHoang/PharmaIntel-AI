// =============================================================================
// Service: CartService
// Chuc nang: Quan ly gio hang theo user.
// Quy tac:
//   - Cap (userId, medicationId) duy nhat - add cung medication thi cong don quantity
//   - Medication phai ton tai (404 NotFound)
//   - Medication phai IsActive de them moi (Conflict 409 neu khac)
//   - Quantity tong (cu + moi) khong vuot StockQuantity (Conflict 409 neu vuot)
//   - Item da co trong cart van xem duoc khi medication bi disable hoac het hang,
//     nhung danh dau IsAvailable=false (FE chan checkout).
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Cart;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;
using PharmaIntel.Infrastructure.Services.Helpers;

namespace PharmaIntel.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly PharmaIntelDbContext _db;

    public CartService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<CartDto> GetAsync(long userId, CancellationToken ct = default)
    {
        var items = await _db.CartItems.AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.AddedAt)
            .Select(c => new CartItemDto
            {
                Id = c.Id,
                MedicationId = c.MedicationId,
                Sku = c.Medication.Sku,
                Name = c.Medication.Name,
                ImageUrl = c.Medication.ImageUrl,
                Manufacturer = c.Medication.Manufacturer,
                UnitPrice = c.Medication.Price,
                DiscountPercent = c.Medication.DiscountPercent,
                Quantity = c.Quantity,
                IsPrescriptionRequired = c.Medication.IsPrescriptionRequired,
                StockQuantity = c.Medication.StockQuantity,
                IsAvailable = c.Medication.IsActive && c.Medication.StockQuantity >= c.Quantity,
                AddedAt = c.AddedAt
            })
            .ToListAsync(ct);

        return new CartDto { Items = items };
    }

    public async Task<CartDto> AddItemAsync(long userId, AddCartItemRequest req, CancellationToken ct = default)
    {
        var med = await _db.Medications.AsNoTracking()
            .Where(m => m.Id == req.MedicationId)
            .Select(m => new { m.Id, m.IsActive, m.StockQuantity })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("thuoc", req.MedicationId);

        if (!med.IsActive)
            throw new ConflictException("Thuoc da ngung kinh doanh");

        var existing = await _db.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.MedicationId == req.MedicationId, ct);

        var newQty = (existing?.Quantity ?? 0) + req.Quantity;
        if (newQty > med.StockQuantity)
            throw new ConflictException(
                $"So luong vuot ton kho. Hien co {med.StockQuantity}, da co {existing?.Quantity ?? 0} trong gio");

        if (newQty > 999)
            throw new ConflictException("So luong toi da 999/san pham");

        if (existing is null)
        {
            var newItem = new CartItem
            {
                UserId = userId,
                MedicationId = req.MedicationId,
                Quantity = req.Quantity,
                AddedAt = DateTime.UtcNow
            };
            _db.CartItems.Add(newItem);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (SqlExceptionHelpers.IsUniqueViolation(ex))
            {
                // Race: tab khac vua insert item cho cung (userId, medicationId).
                // Detach entity loi, load lai row hien tai va cong don quantity.
                _db.Entry(newItem).State = EntityState.Detached;

                var conflicting = await _db.CartItems
                    .FirstAsync(c => c.UserId == userId && c.MedicationId == req.MedicationId, ct);

                var mergedQty = conflicting.Quantity + req.Quantity;
                if (mergedQty > med.StockQuantity)
                    throw new ConflictException(
                        $"So luong vuot ton kho. Hien co {med.StockQuantity}, da co {conflicting.Quantity} trong gio");
                if (mergedQty > 999)
                    throw new ConflictException("So luong toi da 999/san pham");

                conflicting.Quantity = mergedQty;
                conflicting.AddedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
            }
        }
        else
        {
            existing.Quantity = newQty;
            existing.AddedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return await GetAsync(userId, ct);
    }


    public async Task<CartDto> UpdateItemAsync(long userId, long medicationId, UpdateCartItemRequest req, CancellationToken ct = default)
    {
        var item = await _db.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.MedicationId == medicationId, ct)
            ?? throw new NotFoundException("san pham trong gio hang", medicationId);

        var stock = await _db.Medications.Where(m => m.Id == medicationId)
            .Select(m => new { m.StockQuantity, m.IsActive })
            .FirstAsync(ct);

        if (!stock.IsActive)
            throw new ConflictException("Thuoc da ngung kinh doanh - vui long xoa khoi gio");

        if (req.Quantity > stock.StockQuantity)
            throw new ConflictException($"So luong vuot ton kho. Hien co {stock.StockQuantity}");

        item.Quantity = req.Quantity;
        item.AddedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return await GetAsync(userId, ct);
    }

    public async Task<CartDto> RemoveItemAsync(long userId, long medicationId, CancellationToken ct = default)
    {
        var item = await _db.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.MedicationId == medicationId, ct)
            ?? throw new NotFoundException("san pham trong gio hang", medicationId);

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync(ct);

        return await GetAsync(userId, ct);
    }

    public async Task ClearAsync(long userId, CancellationToken ct = default)
    {
        await _db.CartItems.Where(c => c.UserId == userId).ExecuteDeleteAsync(ct);
    }
}
