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
    private readonly PharmaIntelDbContext _db;

    public OrderService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<OrderDto> CheckoutAsync(long userId, CheckoutRequest req, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        // 1. Load cart kem medication tracked (de update stock)
        var cartItems = await _db.CartItems
            .Include(c => c.Medication)
            .Where(c => c.UserId == userId)
            .ToListAsync(ct);

        if (cartItems.Count == 0)
            throw new ConflictException("Gio hang trong - khong the checkout");

        // 2. Validate address
        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == req.AddressId, ct)
                      ?? throw new NotFoundException("dia chi", req.AddressId);
        if (address.UserId != userId)
            throw new ForbiddenException("Dia chi khong thuoc ve ban");
        if (!address.IsActive)
            throw new ConflictException("Dia chi khong con hoat dong");

        // 3. Validate payment method
        var pm = await _db.PaymentMethods.FirstOrDefaultAsync(p => p.Id == req.PaymentMethodId, ct)
                 ?? throw new NotFoundException("phuong thuc thanh toan", req.PaymentMethodId);
        if (pm.UserId != userId)
            throw new ForbiddenException("Phuong thuc thanh toan khong thuoc ve ban");
        if (!pm.IsActive)
            throw new ConflictException("Phuong thuc thanh toan khong con hoat dong");

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

        // 5. Sinh OrderCode duy nhat
        string orderCode = string.Empty;
        for (int attempt = 0; attempt < 5; attempt++)
        {
            var candidate = OrderCodeGenerator.Generate();
            if (!await _db.Orders.AnyAsync(o => o.OrderCode == candidate, ct))
            {
                orderCode = candidate;
                break;
            }
        }
        if (string.IsNullOrEmpty(orderCode))
            throw new ConflictException("Khong sinh duoc ma don sau 5 lan thu - vui long thu lai");

        // 6. Tao Order voi snapshot
        var fullAddress = $"{address.StreetAddress}, {address.Ward}, {address.District}, {address.Province}";
        var paymentStatus = pm.PaymentType == "cod" ? "cod_pending" : "unpaid";

        var order = new Order
        {
            UserId = userId,
            AddressId = address.Id,
            PaymentMethodId = pm.Id,
            PrescriptionId = null,
            OrderCode = orderCode,
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

        // 7. Tao OrderItems + tru ton kho
        foreach (var ci in cartItems)
        {
            var m = ci.Medication;
            var unitFinal = Math.Round(m.Price * (1 - m.DiscountPercent / 100m), 2);
            order.Items.Add(new OrderItem
            {
                MedicationId = m.Id,
                MedicationNameSnapshot = m.Name,
                Quantity = ci.Quantity,
                UnitPrice = m.Price,
                DiscountPercent = m.DiscountPercent,
                TotalPrice = Math.Round(unitFinal * ci.Quantity, 2)
            });

            m.StockQuantity -= ci.Quantity;
            m.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        // 8. Clear cart
        await _db.CartItems.Where(c => c.UserId == userId).ExecuteDeleteAsync(ct);

        await tx.CommitAsync(ct);

        // 9. Return DTO
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

        return new OrderDto
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
    }

    public async Task<OrderDto> CancelMyOrderAsync(long userId, long orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new NotFoundException("don hang", orderId);

        if (order.UserId != userId)
            throw new ForbiddenException("Don hang nay khong thuoc ve ban");

        if (order.Status != "pending")
            throw new ConflictException(
                $"Khong the cancel don dang o trang thai '{order.Status}' - user chi cancel duoc khi 'pending'");

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        order.Status = "cancelled";
        if (order.PaymentStatus == "paid") order.PaymentStatus = "refunded";
        order.UpdatedAt = DateTime.UtcNow;

        await RestoreStockAsync(order, ct);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return await GetByIdAsync(userId, orderId, ct);
    }

    public async Task<OrderDto> AdminUpdateStatusAsync(long orderId, UpdateOrderStatusRequest req, CancellationToken ct = default)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new NotFoundException("don hang", orderId);

        var newStatus = req.Status;
        ValidateAdminTransition(order.Status, newStatus);

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        var wasCancelled = newStatus == "cancelled" && order.Status != "cancelled";

        order.Status = newStatus;
        if (newStatus == "cancelled" && order.PaymentStatus == "paid") order.PaymentStatus = "refunded";
        if (newStatus == "delivered" && order.PaymentTypeSnapshot == "cod") order.PaymentStatus = "paid";
        order.UpdatedAt = DateTime.UtcNow;

        if (wasCancelled)
            await RestoreStockAsync(order, ct);

        await _db.SaveChangesAsync(ct);
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

    private async Task RestoreStockAsync(Order order, CancellationToken ct)
    {
        var medIds = order.Items.Where(i => i.MedicationId.HasValue)
            .Select(i => i.MedicationId!.Value).Distinct().ToList();
        if (medIds.Count == 0) return;

        var meds = await _db.Medications.Where(m => medIds.Contains(m.Id)).ToListAsync(ct);
        var medMap = meds.ToDictionary(m => m.Id);
        foreach (var item in order.Items)
        {
            if (item.MedicationId.HasValue && medMap.TryGetValue(item.MedicationId.Value, out var m))
            {
                m.StockQuantity += item.Quantity;
                m.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
