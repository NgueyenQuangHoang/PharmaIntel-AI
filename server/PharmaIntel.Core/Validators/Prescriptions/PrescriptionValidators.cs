// =============================================================================
// Validators: Prescription + PrescriptionItem create/update.
// Status chi nhan: draft, active, completed, cancelled (user khong duoc set "expired"
// vi day la system-driven theo PrescribedDate, va khong duoc tu set verification).
// =============================================================================
using FluentValidation;
using PharmaIntel.Core.DTOs.Prescriptions;

namespace PharmaIntel.Core.Validators.Prescriptions;

public class PrescriptionCreateRequestValidator : AbstractValidator<PrescriptionCreateRequest>
{
    public PrescriptionCreateRequestValidator()
    {
        RuleFor(x => x.DoctorId)
            .GreaterThan(0).When(x => x.DoctorId.HasValue)
            .WithMessage("DoctorId khong hop le");

        RuleFor(x => x.DoctorNameSnapshot).MaximumLength(255);
        RuleFor(x => x.Title).MaximumLength(255);

        RuleFor(x => x.PrescribedDate)
            .Must(d => !d.HasValue || d.Value <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithMessage("PrescribedDate khong duoc o tuong lai");
    }
}

public class PrescriptionUpdateRequestValidator : AbstractValidator<PrescriptionUpdateRequest>
{
    private static readonly string[] AllowedStatuses = ["draft", "active", "completed", "cancelled"];

    public PrescriptionUpdateRequestValidator()
    {
        RuleFor(x => x.DoctorId)
            .GreaterThan(0).When(x => x.DoctorId.HasValue)
            .WithMessage("DoctorId khong hop le");

        RuleFor(x => x.DoctorNameSnapshot).MaximumLength(255);
        RuleFor(x => x.Title).MaximumLength(255);

        RuleFor(x => x.PrescribedDate)
            .Must(d => !d.HasValue || d.Value <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithMessage("PrescribedDate khong duoc o tuong lai");

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => AllowedStatuses.Contains(s))
            .WithMessage("Status phai la mot trong: draft, active, completed, cancelled");
    }
}

public class PrescriptionItemCreateRequestValidator : AbstractValidator<PrescriptionItemCreateRequest>
{
    public PrescriptionItemCreateRequestValidator()
    {
        RuleFor(x => x.MedicationId)
            .GreaterThan(0).When(x => x.MedicationId.HasValue)
            .WithMessage("MedicationId khong hop le");

        // MedicationName bat buoc khi khong co MedicationId
        RuleFor(x => x.MedicationName)
            .NotEmpty()
                .When(x => !x.MedicationId.HasValue)
                .WithMessage("MedicationName la bat buoc khi khong chon thuoc tu danh muc")
            .MaximumLength(255);

        RuleFor(x => x.Dosage).MaximumLength(100);
        RuleFor(x => x.Frequency).MaximumLength(100);
        RuleFor(x => x.Duration).MaximumLength(100);
    }
}

public class PrescriptionItemUpdateRequestValidator : AbstractValidator<PrescriptionItemUpdateRequest>
{
    public PrescriptionItemUpdateRequestValidator()
    {
        RuleFor(x => x.MedicationId)
            .GreaterThan(0).When(x => x.MedicationId.HasValue)
            .WithMessage("MedicationId khong hop le");

        RuleFor(x => x.MedicationName)
            .NotEmpty()
                .When(x => !x.MedicationId.HasValue)
                .WithMessage("MedicationName la bat buoc khi khong chon thuoc tu danh muc")
            .MaximumLength(255);

        RuleFor(x => x.Dosage).MaximumLength(100);
        RuleFor(x => x.Frequency).MaximumLength(100);
        RuleFor(x => x.Duration).MaximumLength(100);
    }
}
