// =============================================================================
// Service: OrderService
// Chuc nang: Quan ly don hang - checkout (cart -> order), list, detail, update status.
// Quy tac:
//   - Cart phai khong rong, moi medication active, du ton kho
//   - Address va PaymentMethod phai thuoc ve user dang dang nhap
//   - Snapshot price/discount/name vao OrderItem; address/phone/payment_type vao Order
//   - Tru ton kho atomic trong transaction; clear cart sau khi tao order
//   - User chi tu cancel duoc don o trang thai pending
// =============================================================================
using System.Data;
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Orders;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;
using PharmaIntel.Infrastructure.Services.Helpers;

namespace PharmaIntel.Infrastructure.Services;

public class OrderService : IOrderService
{
    private const decimal ShippingFeeFlat = 30000m;
    private static readonly string[] AllowedPaymentTypes = ["cod", "bank_transfer"];
    private readonly PharmaIntelDbContext _db;
    private readonly IVietQrService _vietQr;

    public OrderService(PharmaIntelDbContext db, IVietQrService vietQr)
    {
        _db = db;
        _vietQr = vietQr;
    }

    public async Task<OrderDto> CheckoutAsync(long userId, CheckoutRequest req, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        // 1. Load cart kem medication tracked (de update stock).
        // Snapshot lai Id de buoc clear cart chi xoa dung nhung item da checkout —
        // tranh xoa nham item user vua them o tab khac trong khi checkout dang chay.
        var cartItems = await _db.CartItems
            .Include(c => c.Medication)
            .Where(c => c.UserId == userId)
            .ToListAsync(ct);

        if (cartItems.Count == 0)
            throw new ConflictException("Gio hang trong - khong the checkout");

        var checkoutCartItemIds = cartItems.Select(c => c.Id).ToList();

        // 2. Validate address
        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == req.AddressId, ct)
                      ?? throw new NotFoundException("dia chi", req.AddressId);
        if (address.UserId != userId)
            throw new ForbiddenException("Dia chi khong thuoc ve ban");
        if (!address.IsActive)
            throw new ConflictException("Dia chi khong con hoat dong");

        // 3. Validate / auto-ensure payment method
        // MVP: neu FE khong truyen PaymentMethodId -> ensure 1 method theo PaymentType (mac dinh "cod").
        PaymentMethod pm;
        if (req.PaymentMethodId is null or 0)
        {
            var type = string.IsNullOrWhiteSpace(req.PaymentType) ? "cod" : req.PaymentType.Trim().ToLowerInvariant();
            if (!AllowedPaymentTypes.Contains(type))
                throw new ConflictException($"PaymentType '{type}' khong duoc ho tro. Cho phep: {string.Join(", ", AllowedPaymentTypes)}");
            pm = await EnsurePaymentMethodAsync(userId, type, ct);
        }
        else
        {
            pm = await _db.PaymentMethods.FirstOrDefaultAsync(p => p.Id == req.PaymentMethodId.Value, ct)
                 ?? throw new NotFoundException("phuong thuc thanh toan", req.PaymentMethodId.Value);
            if (pm.UserId != userId)
                throw new ForbiddenException("Phuong thuc thanh toan khong thuoc ve ban");
            if (!pm.IsActive)
                throw new ConflictException("Phuong thuc thanh toan khong con hoat dong");
        }

        // 3b. Validate prescription neu gio co thuoc ke don
        // Map medicationId -> prescription_item_id de gan vao OrderItem sau.
        var rxRequiredCart = cartItems.Where(ci => ci.Medication.IsPrescriptionRequired).ToList();
        Prescription? prescription = null;
        var prescriptionItemMap = new Dictionary<long, long>(); // medicationId -> prescriptionItemId

        if (rxRequiredCart.Count > 0)
        {
            if (req.PrescriptionId is null or 0)
                throw new ConflictException(
                    "Gio hang co thuoc ke don - vui long chon don thuoc da duoc duoc si xac minh");

            prescription = await _db.Prescriptions
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == req.PrescriptionId.Value, ct)
                ?? throw new NotFoundException("don thuoc", req.PrescriptionId.Value);

            if (prescription.UserId != userId)
                throw new ForbiddenException("Don thuoc khong thuoc ve ban");
            if (prescription.VerificationStatus != "verified")
                throw new ConflictException(
                    $"Don thuoc chua duoc xac minh (trang thai: '{prescription.VerificationStatus}')");
            if (prescription.Status is not ("draft" or "active"))
                throw new ConflictException(
                    $"Don thuoc khong con hieu luc (trang thai: '{prescription.Status}')");

            // Single-use: neu bat ky item nao trong don da dispense -> don da duoc dung roi.
            if (prescription.Items.Any(pi => pi.IsDispensed))
                throw new ConflictException(
                    "Don thuoc nay da duoc su dung de mua thuoc - vui long lien he duoc si neu can mua lai");

            // Build map cua nhung medication ke don co trong prescription
            foreach (var pi in prescription.Items)
            {
                if (pi.MedicationId.HasValue)
                    prescriptionItemMap[pi.MedicationId.Value] = pi.Id;
            }

            // Moi thuoc ke don trong gio phai co mat trong prescription
            foreach (var ci in rxRequiredCart)
            {
                if (!prescriptionItemMap.ContainsKey(ci.Medication.Id))
                    throw new ConflictException(
                        $"Thuoc '{ci.Medication.Name}' khong co trong don thuoc da chon");
            }
        }

        // 4. Validate stock + tinh subtotal
        decimal subtotal = 0m;
        foreach (var ci in cartItems)
        {
            var m = ci.Medication;
            if (!m.IsActive)
                throw new ConflictException($"Thuoc '{m.Name}' da ngung kinh doanh");
            if (m.StockQuantity < ci.Quantity)
                throw new ConflictException(
                    $"Thuoc '{m.Name}' khong du ton kho. Hien co {m.StockQuantity}, can {ci.Quantity}");

            var lineTotal = Math.Round(m.Price * (1 - m.DiscountPercent / 100m) * ci.Quantity, 2);
            subtotal += lineTotal;
        }

        var total = subtotal + ShippingFeeFlat;

        // 5. Tao Order voi snapshot (OrderCode se gan trong retry loop o buoc 8)
        var fullAddress = string.IsNullOrWhiteSpace(address.District)
            ? $"{address.StreetAddress}, {address.Ward}, {address.Province}"
            : $"{address.StreetAddress}, {address.Ward}, {address.District}, {address.Province}";
        var paymentStatus = pm.PaymentType == "cod" ? "cod_pending" : "unpaid";

        var order = new Order
        {
            UserId = userId,
            AddressId = address.Id,
            PaymentMethodId = pm.Id,
            PrescriptionId = prescription?.Id,
            OrderCode = string.Empty, // overwritten in save-retry below
            Subtotal = subtotal,
            ShippingFee = ShippingFeeFlat,
            Total = total,
            ShippingRecipientName = address.RecipientName,
            ShippingPhone = address.Phone,
            ShippingFullAddress = fullAddress,
            PaymentTypeSnapshot = pm.PaymentType,
            PaymentStatus = paymentStatus,
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Orders.Add(order);

        // 7. Tao OrderItems + tru ton kho atomic
        // Dieu kien StockQuantity >= ci.Quantity trong cau UPDATE dam bao chi 1 request
        // thanh cong khi nhieu user cung mua san pham cuoi cung (race condition oversell).
        foreach (var ci in cartItems)
        {
            var m = ci.Medication;
            var unitFinal = Math.Round(m.Price * (1 - m.DiscountPercent / 100m), 2);

            var qty = ci.Quantity;
            var now = DateTime.UtcNow;
            var affected = await _db.Medications
                .Where(x => x.Id == m.Id && x.IsActive && x.StockQuantity >= qty)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.StockQuantity, x => x.StockQuantity - qty)
                    .SetProperty(x => x.UpdatedAt, now),
                    ct);

            if (affected != 1)
                throw new ConflictException(
                    $"Thuoc '{m.Name}' khong du ton kho hoac da ngung kinh doanh. Vui long cap nhat gio hang.");

            // Gan PrescriptionItemId neu thuoc nay co trong prescription (FK SetNull cho phep null)
            long? prescriptionItemId = null;
            if (m.IsPrescriptionRequired && prescriptionItemMap.TryGetValue(m.Id, out var piId))
                prescriptionItemId = piId;

            order.Items.Add(new OrderItem
            {
                MedicationId = m.Id,
                PrescriptionItemId = prescriptionItemId,
                MedicationNameSnapshot = m.Name,
                Quantity = ci.Quantity,
                UnitPrice = m.Price,
                DiscountPercent = m.DiscountPercent,
                TotalPrice = Math.Round(unitFinal * ci.Quantity, 2)
            });
        }

        // 7b. Mark prescription items dispensed atomic.
        // Dieu kien IsDispensed = false dam bao 2 request song song dung 1 don
        // chi 1 ben thanh cong (ben kia se rollback toan bo transaction).
        if (prescription is not null && prescriptionItemMap.Count > 0)
        {
            var rxItemIds = prescriptionItemMap.Values.ToList();
            var affectedRx = await _db.PrescriptionItems
                .Where(pi => rxItemIds.Contains(pi.Id) && !pi.IsDispensed)
                .ExecuteUpdateAsync(s => s.SetProperty(pi => pi.IsDispensed, true), ct);

            if (affectedRx != rxItemIds.Count)
                throw new ConflictException(
                    "Don thuoc vua duoc su dung boi request khac - vui long lam moi va kiem tra lai");
        }

        // 8. Save order voi retry tren OrderCode collision.
        // OrderCodeGenerator dung 6 ky tu random/ngay -> xac suat trung cuc thap,
        // nhung neu dung UQ_orders_order_code thi sinh lai code va retry toi da 3 lan.
        const int MaxOrderCodeRetries = 3;
        for (int attempt = 0; attempt < MaxOrderCodeRetries; attempt++)
        {
            order.OrderCode = OrderCodeGenerator.Generate();
            try
            {
                await _db.SaveChangesAsync(ct);
                break;
            }
            catch (DbUpdateException ex) when (
                SqlExceptionHelpers.IsUniqueViolation(ex, "UQ_orders_order_code")
                && attempt < MaxOrderCodeRetries - 1)
            {
                // EF giu entity Added state sau DbUpdateException -> sinh code moi va thu lai.
            }
        }

        // 9. Clear cart — chi xoa nhung item da duoc tinh vao don nay.
        await _db.CartItems
            .Where(c => c.UserId == userId && checkoutCartItemIds.Contains(c.Id))
            .ExecuteDeleteAsync(ct);

        await tx.CommitAsync(ct);

        // 10. Return DTO
        return await GetByIdAsync(userId, order.Id, ct);
    }

    public async Task<PagedResult<OrderListItemDto>> ListMyAsync(long userId, OrderListQuery q, CancellationToken ct = default)
    {
        q.Normalize();

        var query = _db.Orders.AsNoTracking().Where(o => o.UserId == userId);
        if (!string.IsNullOrEmpty(q.Status)) query = query.Where(o => o.Status == q.Status);
        if (!string.IsNullOrEmpty(q.PaymentStatus)) query = query.Where(o => o.PaymentStatus == q.PaymentStatus);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(o => new OrderListItemDto
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                Subtotal = o.Subtotal,
                ShippingFee = o.ShippingFee,
                Total = o.Total,
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                PaymentTypeSnapshot = o.PaymentTypeSnapshot,
                ItemCount = o.Items.Count,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<OrderListItemDto>
        {
            Items = items,
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    public async Task<PagedResult<AdminOrderListItemDto>> AdminListAllAsync(OrderListQuery q, CancellationToken ct = default)
    {
        q.Normalize();

        var query = _db.Orders.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(q.Status)) query = query.Where(o => o.Status == q.Status);
        if (!string.IsNullOrEmpty(q.PaymentStatus)) query = query.Where(o => o.PaymentStatus == q.PaymentStatus);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(o => new AdminOrderListItemDto
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                Subtotal = o.Subtotal,
                ShippingFee = o.ShippingFee,
                Total = o.Total,
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                PaymentTypeSnapshot = o.PaymentTypeSnapshot,
                ItemCount = o.Items.Count,
                CreatedAt = o.CreatedAt,
                UserId = o.UserId,
                UserFullName = o.User.FullName,
                UserEmail = o.User.Email
            })
            .ToListAsync(ct);

        return new PagedResult<AdminOrderListItemDto>
        {
            Items = items,
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    public async Task<OrderDto> GetByIdAsync(long userId, long orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders.AsNoTracking()
            .Where(o => o.Id == orderId)
            .Select(o => new
            {
                o.Id,
                o.UserId,
                o.OrderCode,
                o.AddressId,
                o.PaymentMethodId,
                o.Subtotal,
                o.ShippingFee,
                o.Total,
                o.Status,
                o.PaymentStatus,
                o.PaymentTypeSnapshot,
                o.ShippingRecipientName,
                o.ShippingPhone,
                o.ShippingFullAddress,
                o.CreatedAt,
                o.UpdatedAt,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    MedicationId = i.MedicationId,
                    MedicationName = i.MedicationNameSnapshot,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    DiscountPercent = i.DiscountPercent,
                    TotalPrice = i.TotalPrice
                }).ToList()
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("don hang", orderId);

        if (order.UserId != userId)
            throw new ForbiddenException("Don hang nay khong thuoc ve ban");

        var dto = new OrderDto
        {
            Id = order.Id,
            OrderCode = order.OrderCode,
            AddressId = order.AddressId,
            PaymentMethodId = order.PaymentMethodId,
            Subtotal = order.Subtotal,
            ShippingFee = order.ShippingFee,
            Total = order.Total,
            Status = order.Status,
            PaymentStatus = order.PaymentStatus,
            PaymentTypeSnapshot = order.PaymentTypeSnapshot,
            ShippingRecipientName = order.ShippingRecipientName,
            ShippingPhone = order.ShippingPhone,
            ShippingFullAddress = order.ShippingFullAddress,
            ItemCount = order.Items.Count,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items
        };

        // Sinh QR khi user can chuyen khoan va don chua duoc thanh toan / chua bi huy.
        if (order.PaymentTypeSnapshot == "bank_transfer"
            && order.PaymentStatus is not ("paid" or "refunded")
            && order.Status != "cancelled")
        {
            dto.VietQrUrl = _vietQr.CreateQrUrl(order.Total, order.OrderCode);
            dto.TransferContent = _vietQr.CreateTransferContent(order.OrderCode);
        }

        return dto;
    }

    public async Task<OrderDto> CancelMyOrderAsync(long userId, long orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new NotFoundException("don hang", orderId);

        if (order.UserId != userId)
            throw new ForbiddenException("Don hang nay khong thuoc ve ban");

        if (order.Status != "pending")
            throw new ConflictException(
                $"Khong the cancel don dang o trang thai '{order.Status}' - user chi cancel duoc khi 'pending'");

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        // Chuyen trang thai atomic: chi 1 request thanh cong khi 2 tab cung bam cancel.
        var now = DateTime.UtcNow;
        var affected = await _db.Orders
            .Where(o => o.Id == orderId && o.UserId == userId && o.Status == "pending")
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(o => o.Status, "cancelled")
                .SetProperty(o => o.PaymentStatus, o => o.PaymentStatus == "paid" ? "refunded" : o.PaymentStatus)
                .SetProperty(o => o.UpdatedAt, now),
                ct);

        if (affected != 1)
            throw new ConflictException("Don hang khong con o trang thai 'pending' - co the da bi cancel boi request khac");

        await RestoreStockAsync(order, ct);
        await RestorePrescriptionItemsAsync(order, ct);
        await tx.CommitAsync(ct);

        return await GetByIdAsync(userId, orderId, ct);
    }

    public async Task<OrderDto> AdminUpdateStatusAsync(long orderId, UpdateOrderStatusRequest req, CancellationToken ct = default)
    {
        var order = await _db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new NotFoundException("don hang", orderId);

        var newStatus = req.Status;
        var currentStatus = order.Status;
        ValidateAdminTransition(currentStatus, newStatus);

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        var wasCancelled = newStatus == "cancelled" && currentStatus != "cancelled";
        var paymentTypeSnapshot = order.PaymentTypeSnapshot;
        var now = DateTime.UtcNow;

        // Atomic transition: chi thanh cong khi status van la `currentStatus` —
        // chong 2 admin cung chuyen trang thai don gay double-restore stock.
        var affected = await _db.Orders
            .Where(o => o.Id == orderId && o.Status == currentStatus)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(o => o.Status, newStatus)
                .SetProperty(o => o.PaymentStatus, o =>
                    (newStatus == "cancelled" && o.PaymentStatus == "paid") ? "refunded" :
                    (newStatus == "delivered" && paymentTypeSnapshot == "cod") ? "paid" :
                    o.PaymentStatus)
                .SetProperty(o => o.UpdatedAt, now),
                ct);

        if (affected != 1)
            throw new ConflictException(
                $"Trang thai don da thay doi - khong the chuyen tu '{currentStatus}' sang '{newStatus}'");

        if (wasCancelled)
        {
            await RestoreStockAsync(order, ct);
            await RestorePrescriptionItemsAsync(order, ct);
        }

        await tx.CommitAsync(ct);

        return await GetByIdAsync(order.UserId, orderId, ct);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static void ValidateAdminTransition(string current, string next)
    {
        if (current == next) return;

        // State machine cho admin
        var allowed = current switch
        {
            "pending"    => new[] { "confirmed", "cancelled" },
            "confirmed"  => new[] { "processing", "cancelled" },
            "processing" => new[] { "shipping", "cancelled" },
            "shipping"   => new[] { "delivered", "cancelled" },
            "delivered"  => new[] { "refunded" },
            "cancelled"  => Array.Empty<string>(),
            "refunded"   => Array.Empty<string>(),
            _            => Array.Empty<string>()
        };

        if (!allowed.Contains(next))
            throw new ConflictException($"Khong the chuyen tu '{current}' sang '{next}'");
    }

    // Dam bao user co 1 PaymentMethod theo `type` (cod/bank_transfer) active.
    // Lay cai san co hoac tao moi. Goi trong CheckoutAsync khi FE khong truyen PaymentMethodId.
    private async Task<PaymentMethod> EnsurePaymentMethodAsync(long userId, string type, CancellationToken ct)
    {
        var existing = await _db.PaymentMethods
            .FirstOrDefaultAsync(p => p.UserId == userId && p.PaymentType == type && p.IsActive, ct);
        if (existing is not null) return existing;

        var displayName = type switch
        {
            "cod" => "Thanh toan khi nhan hang",
            "bank_transfer" => "Chuyen khoan ngan hang",
            _ => type
        };

        var pm = new PaymentMethod
        {
            UserId = userId,
            PaymentType = type,
            DisplayName = displayName,
            IsDefault = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.PaymentMethods.Add(pm);
        await _db.SaveChangesAsync(ct);
        return pm;
    }

    private async Task RestoreStockAsync(Order order, CancellationToken ct)
    {
        // Cong lai ton kho atomic cho tung item. Goi sau khi status order da chuyen
        // sang `cancelled` thanh cong qua atomic UPDATE — dam bao chi chay 1 lan.
        var now = DateTime.UtcNow;
        foreach (var item in order.Items)
        {
            if (!item.MedicationId.HasValue) continue;
            var medId = item.MedicationId.Value;
            var qty = item.Quantity;
            await _db.Medications
                .Where(m => m.Id == medId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.StockQuantity, m => m.StockQuantity + qty)
                    .SetProperty(m => m.UpdatedAt, now),
                    ct);
        }
    }

    private async Task RestorePrescriptionItemsAsync(Order order, CancellationToken ct)
    {
        // Restore single-use flag cho cac prescription_items lien quan toi order bi cancel.
        // Chi goi khi cancel tu pre-delivered states -> don thuoc chua thuc su duoc cap thuoc.
        var rxItemIds = order.Items
            .Where(i => i.PrescriptionItemId.HasValue)
            .Select(i => i.PrescriptionItemId!.Value)
            .Distinct()
            .ToList();
        if (rxItemIds.Count == 0) return;

        await _db.PrescriptionItems
            .Where(pi => rxItemIds.Contains(pi.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(pi => pi.IsDispensed, false), ct);
    }
}
