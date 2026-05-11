// =============================================================================
// DTOs: CheckoutRequest, UpdateOrderStatusRequest, OrderListQuery
// =============================================================================
using PharmaIntel.Core.DTOs.Common;

namespace PharmaIntel.Core.DTOs.Orders;

public class CheckoutRequest
{
    public long AddressId { get; set; }

    // Null/0 = backend tu dam bao 1 PaymentMethod theo PaymentType cho user.
    // Khi tich hop gateway, FE truyen Id cua method da luu.
    public long? PaymentMethodId { get; set; }

    // "cod" (mac dinh) | "bank_transfer". Backend ensure-or-create PaymentMethod tuong ung.
    // Khong dung khi PaymentMethodId co gia tri (ID quyet dinh).
    public string? PaymentType { get; set; }

    // Bat buoc khi gio hang co thuoc ke don (Medication.IsPrescriptionRequired = true).
    // Prescription phai thuoc user, status in ('draft','active'),
    // verification_status='verified', va cac thuoc ke don trong gio phai khop
    // voi PrescriptionItem.
    public long? PrescriptionId { get; set; }
}

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class OrderListQuery : PagedQuery
{
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
}
