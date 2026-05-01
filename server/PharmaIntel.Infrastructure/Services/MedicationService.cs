// =============================================================================
// Service: MedicationService
// Chuc nang: Nghiep vu CRUD thuoc.
// Quy tac:
//   - Sku duy nhat. Trung -> ConflictException(409).
//   - CategoryId phai ton tai (NotFoundException neu khong co).
//   - Khong cho xoa cung neu thuoc dang co trong CartItem/OrderItem/PrescriptionItem
//     hoac DiagnosticResultMedication (FK Restrict). Tra 409.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Medications;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class MedicationService : IMedicationService
{
    private readonly PharmaIntelDbContext _db;

    public MedicationService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<MedicationListItemDto>> ListAsync(MedicationListQuery q, CancellationToken ct = default)
    {
        q.Normalize();

        var query = _db.Medications.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Q))
        {
            var k = q.Q.Trim().ToLower();
            query = query.Where(m =>
                m.Name.ToLower().Contains(k) ||
                m.Sku.ToLower().Contains(k) ||
                (m.GenericName != null && m.GenericName.ToLower().Contains(k)));
        }
        if (q.CategoryId.HasValue) query = query.Where(m => m.CategoryId == q.CategoryId);
        if (q.IsActive.HasValue) query = query.Where(m => m.IsActive == q.IsActive);
        if (q.IsFeatured.HasValue) query = query.Where(m => m.IsFeatured == q.IsFeatured);
        if (q.IsBestSeller.HasValue) query = query.Where(m => m.IsBestSeller == q.IsBestSeller);
        if (q.IsPrescriptionRequired.HasValue) query = query.Where(m => m.IsPrescriptionRequired == q.IsPrescriptionRequired);
        if (q.MinPrice.HasValue) query = query.Where(m => m.Price >= q.MinPrice);
        if (q.MaxPrice.HasValue) query = query.Where(m => m.Price <= q.MaxPrice);

        query = (q.SortBy?.ToLowerInvariant()) switch
        {
            "price" => q.SortDesc ? query.OrderByDescending(m => m.Price) : query.OrderBy(m => m.Price),
            "createdat" => q.SortDesc ? query.OrderByDescending(m => m.CreatedAt) : query.OrderBy(m => m.CreatedAt),
            _ => q.SortDesc ? query.OrderByDescending(m => m.Name) : query.OrderBy(m => m.Name)
        };

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(m => new MedicationListItemDto
            {
                Id = m.Id,
                Sku = m.Sku,
                Name = m.Name,
                GenericName = m.GenericName,
                Manufacturer = m.Manufacturer,
                Price = m.Price,
                DiscountPercent = m.DiscountPercent,
                CategoryId = m.CategoryId,
                CategoryName = m.Category.Name,
                ImageUrl = m.ImageUrl,
                IsFeatured = m.IsFeatured,
                IsBestSeller = m.IsBestSeller,
                IsPrescriptionRequired = m.IsPrescriptionRequired,
                StockQuantity = m.StockQuantity,
                IsActive = m.IsActive
            })
            .ToListAsync(ct);

        return new PagedResult<MedicationListItemDto>
        {
            Items = items,
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    public async Task<MedicationDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var dto = await _db.Medications.AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new MedicationDto
            {
                Id = m.Id,
                Sku = m.Sku,
                Name = m.Name,
                GenericName = m.GenericName,
                Manufacturer = m.Manufacturer,
                RegistrationNumber = m.RegistrationNumber,
                Description = m.Description,
                Dosage = m.Dosage,
                Packaging = m.Packaging,
                Price = m.Price,
                DiscountPercent = m.DiscountPercent,
                CategoryId = m.CategoryId,
                CategoryName = m.Category.Name,
                UsageInstructions = m.UsageInstructions,
                Benefits = m.Benefits,
                ActiveIngredients = m.ActiveIngredients,
                Contraindications = m.Contraindications,
                SideEffects = m.SideEffects,
                StorageInstructions = m.StorageInstructions,
                ImageUrl = m.ImageUrl,
                IsFeatured = m.IsFeatured,
                IsBestSeller = m.IsBestSeller,
                IsPrescriptionRequired = m.IsPrescriptionRequired,
                StockQuantity = m.StockQuantity,
                IsActive = m.IsActive,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        return dto ?? throw new NotFoundException("thuoc", id);
    }

    public async Task<MedicationDto> CreateAsync(MedicationCreateRequest req, CancellationToken ct = default)
    {
        var sku = req.Sku.Trim();

        if (await _db.Medications.AnyAsync(m => m.Sku == sku, ct))
            throw new ConflictException($"SKU '{sku}' da ton tai");

        if (!await _db.Categories.AnyAsync(c => c.Id == req.CategoryId, ct))
            throw new NotFoundException("danh muc", req.CategoryId);

        var entity = new Medication
        {
            Sku = sku,
            Name = req.Name.Trim(),
            GenericName = req.GenericName,
            Manufacturer = req.Manufacturer,
            RegistrationNumber = req.RegistrationNumber,
            Description = req.Description,
            Dosage = req.Dosage,
            Packaging = req.Packaging,
            Price = req.Price,
            DiscountPercent = req.DiscountPercent,
            CategoryId = req.CategoryId,
            UsageInstructions = req.UsageInstructions,
            Benefits = req.Benefits,
            ActiveIngredients = req.ActiveIngredients,
            Contraindications = req.Contraindications,
            SideEffects = req.SideEffects,
            StorageInstructions = req.StorageInstructions,
            ImageUrl = req.ImageUrl,
            IsFeatured = req.IsFeatured,
            IsBestSeller = req.IsBestSeller,
            IsPrescriptionRequired = req.IsPrescriptionRequired,
            StockQuantity = req.StockQuantity,
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Medications.Add(entity);
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<MedicationDto> UpdateAsync(long id, MedicationUpdateRequest req, CancellationToken ct = default)
    {
        var entity = await _db.Medications.FirstOrDefaultAsync(m => m.Id == id, ct)
                     ?? throw new NotFoundException("thuoc", id);

        var sku = req.Sku.Trim();
        if (sku != entity.Sku && await _db.Medications.AnyAsync(m => m.Sku == sku && m.Id != id, ct))
            throw new ConflictException($"SKU '{sku}' da ton tai");

        if (req.CategoryId != entity.CategoryId &&
            !await _db.Categories.AnyAsync(c => c.Id == req.CategoryId, ct))
            throw new NotFoundException("danh muc", req.CategoryId);

        entity.Sku = sku;
        entity.Name = req.Name.Trim();
        entity.GenericName = req.GenericName;
        entity.Manufacturer = req.Manufacturer;
        entity.RegistrationNumber = req.RegistrationNumber;
        entity.Description = req.Description;
        entity.Dosage = req.Dosage;
        entity.Packaging = req.Packaging;
        entity.Price = req.Price;
        entity.DiscountPercent = req.DiscountPercent;
        entity.CategoryId = req.CategoryId;
        entity.UsageInstructions = req.UsageInstructions;
        entity.Benefits = req.Benefits;
        entity.ActiveIngredients = req.ActiveIngredients;
        entity.Contraindications = req.Contraindications;
        entity.SideEffects = req.SideEffects;
        entity.StorageInstructions = req.StorageInstructions;
        entity.ImageUrl = req.ImageUrl;
        entity.IsFeatured = req.IsFeatured;
        entity.IsBestSeller = req.IsBestSeller;
        entity.IsPrescriptionRequired = req.IsPrescriptionRequired;
        entity.StockQuantity = req.StockQuantity;
        entity.IsActive = req.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _db.Medications.FirstOrDefaultAsync(m => m.Id == id, ct)
                     ?? throw new NotFoundException("thuoc", id);

        var refCount = await _db.CartItems.CountAsync(x => x.MedicationId == id, ct)
                     + await _db.OrderItems.CountAsync(x => x.MedicationId == id, ct)
                     + await _db.PrescriptionItems.CountAsync(x => x.MedicationId == id, ct)
                     + await _db.DiagnosticResultMedications.CountAsync(x => x.MedicationId == id, ct);

        if (refCount > 0)
            throw new ConflictException($"Khong the xoa - thuoc dang duoc tham chieu o {refCount} ban ghi (cart/order/prescription/diagnostic)");

        _db.Medications.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

}
