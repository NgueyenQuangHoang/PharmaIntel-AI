// =============================================================================
// Validator: UpdateUserProfileRequestValidator
// =============================================================================
using FluentValidation;
using PharmaIntel.Core.DTOs.Users;

namespace PharmaIntel.Core.Validators.Users;

public class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ho ten la bat buoc")
            .MinimumLength(2).WithMessage("Ho ten toi thieu 2 ky tu")
            .MaximumLength(255).WithMessage("Ho ten toi da 255 ky tu");

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500).WithMessage("Avatar URL toi da 500 ky tu")
            .Must(IsValidUrlOrEmpty).WithMessage("Avatar URL khong hop le");
    }

    private static bool IsValidUrlOrEmpty(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var u)
            && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
    }
}
