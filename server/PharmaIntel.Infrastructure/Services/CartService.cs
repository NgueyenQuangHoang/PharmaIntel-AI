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
                // Detach entity loi, cong don quantity bang atomic update.
                _db.Entry(newItem).State = EntityState.Detached;
                await IncrementCartQuantityAtomicAsync(userId, req.MedicationId, req.Quantity, med.StockQuantity, ct);
            }
        }
        else
        {
            // Cong don quantity bang atomic UPDATE de tranh lost-update khi 2 tab cung tang.
            await IncrementCartQuantityAtomicAsync(userId, req.MedicationId, req.Quantity, med.StockQuantity, ct);
        }

        return await GetAsync(userId, ct);
    }

    // Cong don quantity cho cart item theo (userId, medicationId) bang 1 cau UPDATE atomic.
    // Guard `quantity + add <= maxStock` va `<= 999` nam trong WHERE -> chong lost-update
    // va chong vuot ton kho khi 2 tab cung add. Neu affected = 0, query lai de tra error message
    // chinh xac (vuot stock, vuot 999, hoac san pham vua bi xoa).
    private async Task IncrementCartQuantityAtomicAsync(
        long userId, long medicationId, int add, int maxStock, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var affected = await _db.CartItems
            .Where(c => c.UserId == userId && c.MedicationId == medicationId
                && c.Quantity + add <= maxStock
                && c.Quantity + add <= 999)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Quantity, c => c.Quantity + add)
                .SetProperty(c => c.AddedAt, now), ct);

        if (affected == 1) return;

        var currentQty = await _db.CartItems
            .Where(c => c.UserId == userId && c.MedicationId == medicationId)
            .Select(c => (int?)c.Quantity)
            .FirstOrDefaultAsync(ct);

        if (currentQty is null)
            throw new ConflictException("San pham vua bi xoa khoi gio - vui long lam moi gio hang");
        if (currentQty.Value + add > 999)
            throw new ConflictException("So luong toi da 999/san pham");
        throw new ConflictException(
            $"So luong vuot ton kho. Hien co {maxStock}, da co {currentQty.Value} trong gio");
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
