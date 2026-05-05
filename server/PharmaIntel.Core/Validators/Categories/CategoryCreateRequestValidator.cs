// =============================================================================
// Validator: CategoryCreateRequestValidator
// =============================================================================
using FluentValidation;
using PharmaIntel.Core.DTOs.Categories;

namespace PharmaIntel.Core.Validators.Categories;

public class CategoryCreateRequestValidator : AbstractValidator<CategoryCreateRequest>
{
    public CategoryCreateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ten danh muc la bat buoc")
            .MaximumLength(100);

        RuleFor(x => x.Slug)
            .MaximumLength(100)
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
                .When(x => !string.IsNullOrWhiteSpace(x.Slug))
                .WithMessage("Slug chi gom chu thuong, so va dau gach ngang (vi du: thuoc-cam-cum)");

        RuleFor(x => x.Icon).MaximumLength(100);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("DisplayOrder phai >= 0");
    }
}
