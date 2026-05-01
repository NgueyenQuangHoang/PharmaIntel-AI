// =============================================================================
// Validator: AddressUpdateRequestValidator
// =============================================================================
using FluentValidation;
using PharmaIntel.Core.DTOs.Addresses;

namespace PharmaIntel.Core.Validators.Addresses;

public class AddressUpdateRequestValidator : AbstractValidator<AddressUpdateRequest>
{
    public AddressUpdateRequestValidator()
    {
        RuleFor(x => x.RecipientName)
            .NotEmpty().WithMessage("Ten nguoi nhan la bat buoc")
            .MinimumLength(2).WithMessage("Ten nguoi nhan toi thieu 2 ky tu")
            .MaximumLength(255).WithMessage("Ten nguoi nhan toi da 255 ky tu");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("So dien thoai la bat buoc")
            .MaximumLength(20)
            .Matches(@"^(0|\+84)\d{9,10}$")
                .WithMessage("So dien thoai khong hop le (vi du: 0901234567 hoac +84901234567)");

        RuleFor(x => x.Province)
            .NotEmpty().WithMessage("Tinh/Thanh pho la bat buoc")
            .MaximumLength(100);

        RuleFor(x => x.District)
            .NotEmpty().WithMessage("Quan/Huyen la bat buoc")
            .MaximumLength(100);

        RuleFor(x => x.Ward)
            .NotEmpty().WithMessage("Phuong/Xa la bat buoc")
            .MaximumLength(100);

        RuleFor(x => x.StreetAddress)
            .NotEmpty().WithMessage("Dia chi cu the la bat buoc")
            .MaximumLength(500);
    }
}
