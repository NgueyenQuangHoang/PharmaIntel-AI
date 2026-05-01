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
        RuleFor(x => x.PaymentMethodId).GreaterThan(0).WithMessage("PaymentMethodId la bat buoc");
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
