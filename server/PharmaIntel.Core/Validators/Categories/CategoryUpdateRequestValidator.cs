// =============================================================================
// Validator: CategoryUpdateRequestValidator
// =============================================================================
using FluentValidation;
using PharmaIntel.Core.DTOs.Categories;

namespace PharmaIntel.Core.Validators.Categories;

public class CategoryUpdateRequestValidator : AbstractValidator<CategoryUpdateRequest>
{
    public CategoryUpdateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug la bat buoc")
            .MaximumLength(100)
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
                .WithMessage("Slug chi gom chu thuong, so va dau gach ngang");
        RuleFor(x => x.Icon).MaximumLength(100);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
