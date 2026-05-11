// =============================================================================
// Validators: MedicationReminder + Log create/update.
// Khop CHECK constraint:
//   - frequency_type IN ('once','daily','weekly','custom')
//   - reminder status IN ('active','paused','completed','cancelled')
//   - log status IN ('scheduled','taken','missed','skipped') - user POST chi cho
//     phep 'taken'/'missed'/'skipped' (status 'scheduled' do system tao).
// =============================================================================
using FluentValidation;
using PharmaIntel.Core.DTOs.MedicationReminders;

namespace PharmaIntel.Core.Validators.MedicationReminders;

public class MedicationReminderCreateRequestValidator : AbstractValidator<MedicationReminderCreateRequest>
{
    private static readonly string[] AllowedFrequency = ["once", "daily", "weekly", "custom"];

    public MedicationReminderCreateRequestValidator()
    {
        RuleFor(x => x.PrescriptionItemId)
            .GreaterThan(0).When(x => x.PrescriptionItemId.HasValue)
            .WithMessage("PrescriptionItemId khong hop le");

        // MedicationName bat buoc neu khong co PrescriptionItemId
        RuleFor(x => x.MedicationName)
            .NotEmpty()
                .When(x => !x.PrescriptionItemId.HasValue)
                .WithMessage("MedicationName la bat buoc khi khong link toi don thuoc")
            .MaximumLength(255);

        RuleFor(x => x.FrequencyType)
            .NotEmpty()
            .Must(v => AllowedFrequency.Contains(v))
            .WithMessage("FrequencyType phai la mot trong: once, daily, weekly, custom");

        RuleFor(x => x.EndDate)
            .Must((req, end) => !end.HasValue || !req.StartDate.HasValue || end.Value >= req.StartDate.Value)
            .WithMessage("EndDate phai >= StartDate");
    }
}

public class MedicationReminderUpdateRequestValidator : AbstractValidator<MedicationReminderUpdateRequest>
{
    private static readonly string[] AllowedFrequency = ["once", "daily", "weekly", "custom"];
    private static readonly string[] AllowedStatuses = ["active", "paused", "completed", "cancelled"];

    public MedicationReminderUpdateRequestValidator()
    {
        RuleFor(x => x.PrescriptionItemId)
            .GreaterThan(0).When(x => x.PrescriptionItemId.HasValue)
            .WithMessage("PrescriptionItemId khong hop le");

        RuleFor(x => x.MedicationName)
            .NotEmpty()
                .When(x => !x.PrescriptionItemId.HasValue)
                .WithMessage("MedicationName la bat buoc khi khong link toi don thuoc")
            .MaximumLength(255);

        RuleFor(x => x.FrequencyType)
            .NotEmpty()
            .Must(v => AllowedFrequency.Contains(v))
            .WithMessage("FrequencyType phai la mot trong: once, daily, weekly, custom");

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(v => AllowedStatuses.Contains(v))
            .WithMessage("Status phai la mot trong: active, paused, completed, cancelled");

        RuleFor(x => x.EndDate)
            .Must((req, end) => !end.HasValue || !req.StartDate.HasValue || end.Value >= req.StartDate.Value)
            .WithMessage("EndDate phai >= StartDate");
    }
}

public class MedicationReminderLogCreateRequestValidator : AbstractValidator<MedicationReminderLogCreateRequest>
{
    private static readonly string[] AllowedLogStatuses = ["taken", "missed", "skipped"];

    public MedicationReminderLogCreateRequestValidator()
    {
        RuleFor(x => x.ScheduledAt)
            .NotEmpty().WithMessage("ScheduledAt la bat buoc");

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(v => AllowedLogStatuses.Contains(v))
            .WithMessage("Status phai la mot trong: taken, missed, skipped");
    }
}
