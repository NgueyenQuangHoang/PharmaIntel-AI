// =============================================================================
// Validator: ChangePasswordRequestValidator
// Tai su dung quy tac mat khau giong RegisterRequestValidator.
// =============================================================================
using FluentValidation;
using PharmaIntel.Core.DTOs.Users;

namespace PharmaIntel.Core.Validators.Users;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mat khau hien tai la bat buoc");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Mat khau moi la bat buoc")
            .MinimumLength(8).WithMessage("Mat khau toi thieu 8 ky tu")
            .MaximumLength(100).WithMessage("Mat khau toi da 100 ky tu")
            .Matches(@"[A-Z]").WithMessage("Mat khau phai co it nhat 1 chu HOA")
            .Matches(@"[a-z]").WithMessage("Mat khau phai co it nhat 1 chu thuong")
            .Matches(@"\d").WithMessage("Mat khau phai co it nhat 1 chu so")
            .NotEqual(x => x.CurrentPassword).WithMessage("Mat khau moi phai khac mat khau hien tai");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Xac nhan mat khau la bat buoc")
            .Equal(x => x.NewPassword).WithMessage("Xac nhan mat khau khong khop");
    }
}
