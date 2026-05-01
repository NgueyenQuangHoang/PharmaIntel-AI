// =============================================================================
// Validator: LoginRequestValidator
// Chuc nang: Validation rules cho LoginRequest.
// =============================================================================
using FluentValidation;
using PharmaIntel.Core.DTOs.Auth;

namespace PharmaIntel.Core.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email la bat buoc")
            .EmailAddress().WithMessage("Email khong hop le");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mat khau la bat buoc");
    }
}
