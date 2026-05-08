// =============================================================================
// Validators: CheckoutRequest, UpdateOrderStatusRequest
// =============================================================================
using FluentValidation;
using PharmaIntel.Core.DTOs.Orders;

namespace PharmaIntel.Core.Validators.Orders;

public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        RuleFor(x => x.AddressId).GreaterThan(0).WithMessage("AddressId la bat buoc");
        // PaymentMethodId optional: null = COD mac dinh; nguoc lai phai > 0
        RuleFor(x => x.PaymentMethodId!.Value)
            .GreaterThan(0).WithMessage("PaymentMethodId phai > 0")
            .When(x => x.PaymentMethodId.HasValue);
    }
}

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    private static readonly string[] AllowedStatuses =
        ["pending", "confirmed", "processing", "shipping", "delivered", "cancelled", "refunded"];

    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status la bat buoc")
            .Must(s => AllowedStatuses.Contains(s))
                .WithMessage($"Status phai la 1 trong: {string.Join(", ", AllowedStatuses)}");
    }
}
