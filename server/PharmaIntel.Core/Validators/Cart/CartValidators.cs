// =============================================================================
// Validators: AddCartItemRequest, UpdateCartItemRequest
// =============================================================================
using FluentValidation;
using PharmaIntel.Core.DTOs.Cart;

namespace PharmaIntel.Core.Validators.Cart;

public class AddCartItemRequestValidator : AbstractValidator<AddCartItemRequest>
{
    public AddCartItemRequestValidator()
    {
        RuleFor(x => x.MedicationId).GreaterThan(0).WithMessage("MedicationId la bat buoc");
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("So luong phai > 0")
            .LessThanOrEqualTo(999).WithMessage("So luong toi da 999");
    }
}

public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
{
    public UpdateCartItemRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("So luong phai > 0")
            .LessThanOrEqualTo(999).WithMessage("So luong toi da 999");
    }
}
