// =============================================================================
// Validators: Medication Create / Update
// Chuc nang: Dung generic base de re-use rules giua Create va Update.
// =============================================================================
using FluentValidation;
using PharmaIntel.Core.DTOs.Medications;

namespace PharmaIntel.Core.Validators.Medications;

public abstract class MedicationFieldsValidator<T> : AbstractValidator<T>
    where T : MedicationCreateRequest
{
    protected MedicationFieldsValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.GenericName).MaximumLength(255);
        RuleFor(x => x.Manufacturer).MaximumLength(255);
        RuleFor(x => x.RegistrationNumber).MaximumLength(100);
        RuleFor(x => x.Dosage).MaximumLength(100);
        RuleFor(x => x.Packaging).MaximumLength(100);
        RuleFor(x => x.ImageUrl).MaximumLength(500);

        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Gia phai >= 0");
        RuleFor(x => x.DiscountPercent)
            .InclusiveBetween(0, 100).WithMessage("DiscountPercent phai trong khoang 0-100");
        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("StockQuantity phai >= 0");
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId la bat buoc");
    }
}

public class MedicationCreateRequestValidator : MedicationFieldsValidator<MedicationCreateRequest> { }
