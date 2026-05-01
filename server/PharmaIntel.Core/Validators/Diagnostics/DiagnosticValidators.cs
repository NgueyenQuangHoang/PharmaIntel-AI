// =============================================================================
// Validators: Diagnostic session create + message add.
// =============================================================================
using FluentValidation;
using PharmaIntel.Core.DTOs.Diagnostics;

namespace PharmaIntel.Core.Validators.Diagnostics;

public class CreateDiagnosticSessionRequestValidator : AbstractValidator<CreateDiagnosticSessionRequest>
{
    public CreateDiagnosticSessionRequestValidator()
    {
        RuleFor(x => x.SymptomIds)
            .NotNull()
            .Must(ids => ids != null && ids.Count > 0)
                .WithMessage("Phai chon it nhat 1 trieu chung")
            .Must(ids => ids == null || ids.All(id => id > 0))
                .WithMessage("SymptomIds chua gia tri khong hop le")
            .Must(ids => ids == null || ids.Count <= 20)
                .WithMessage("Toi da 20 trieu chung trong 1 phien");

        RuleFor(x => x.InitialMessage)
            .MaximumLength(2000).WithMessage("InitialMessage toi da 2000 ky tu");
    }
}

public class AddDiagnosticMessageRequestValidator : AbstractValidator<AddDiagnosticMessageRequest>
{
    public AddDiagnosticMessageRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Noi dung tin nhan la bat buoc")
            .MaximumLength(2000).WithMessage("Tin nhan toi da 2000 ky tu");
    }
}
