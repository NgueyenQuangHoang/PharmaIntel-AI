// =============================================================================
// Validator: RegisterRequestValidator
// Chuc nang: Tach validation rules cua RegisterRequest ra khoi DTO bang FluentValidation.
// =============================================================================
using FluentValidation;
using PharmaIntel.Core.DTOs.Auth;

namespace PharmaIntel.Core.Validators.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ho ten la bat buoc")
            .MinimumLength(2).WithMessage("Ho ten toi thieu 2 ky tu")
            .MaximumLength(255).WithMessage("Ho ten toi da 255 ky tu");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email la bat buoc")
            .EmailAddress().WithMessage("Email khong hop le")
            .MaximumLength(255);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mat khau la bat buoc")
            .MinimumLength(8).WithMessage("Mat khau toi thieu 8 ky tu")
            .MaximumLength(100).WithMessage("Mat khau toi da 100 ky tu")
            .Matches(@"[A-Z]").WithMessage("Mat khau phai co it nhat 1 chu HOA")
            .Matches(@"[a-z]").WithMessage("Mat khau phai co it nhat 1 chu thuong")
            .Matches(@"\d").WithMessage("Mat khau phai co it nhat 1 chu so");

        RuleFor(x => x.IsTermsAccepted)
            .Equal(true).WithMessage("Phai dong y dieu khoan su dung");
    }
}
