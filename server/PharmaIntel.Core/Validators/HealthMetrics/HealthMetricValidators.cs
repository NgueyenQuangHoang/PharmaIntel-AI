// =============================================================================
// Validators: HealthMetric create/update.
// Khop CHECK constraint:
//   metric_type IN ('blood_pressure','heart_rate','temperature','weight','blood_sugar','oxygen_saturation')
// Theo y khoa, gioi han hop ly cho moi loai chi so:
//   blood_pressure: tam thu 50-300, tam truong 30-200, ValueNumber2 BAT BUOC
//   heart_rate:     30-250 bpm
//   temperature:    30-45 (Celsius)
//   weight:         0.5-500 kg
//   blood_sugar:    1-50 mmol/L
//   oxygen_saturation: 50-100 %
// =============================================================================
using FluentValidation;
using PharmaIntel.Core.DTOs.HealthMetrics;

namespace PharmaIntel.Core.Validators.HealthMetrics;

internal static class HealthMetricRules
{
    internal static readonly string[] AllowedTypes =
        ["blood_pressure", "heart_rate", "temperature", "weight", "blood_sugar", "oxygen_saturation"];

    internal static (decimal Min, decimal Max) RangeOf(string metricType) => metricType switch
    {
        "blood_pressure"    => (50m, 300m),
        "heart_rate"        => (30m, 250m),
        "temperature"       => (30m, 45m),
        "weight"            => (0.5m, 500m),
        "blood_sugar"       => (1m, 50m),
        "oxygen_saturation" => (50m, 100m),
        _ => (decimal.MinValue, decimal.MaxValue)
    };
}

public class HealthMetricCreateRequestValidator : AbstractValidator<HealthMetricCreateRequest>
{
    public HealthMetricCreateRequestValidator()
    {
        RuleFor(x => x.MetricType)
            .NotEmpty()
            .Must(v => HealthMetricRules.AllowedTypes.Contains(v))
            .WithMessage("MetricType phai la mot trong: blood_pressure, heart_rate, temperature, weight, blood_sugar, oxygen_saturation");

        RuleFor(x => x.ValueNumber)
            .Must((req, value) =>
            {
                if (!HealthMetricRules.AllowedTypes.Contains(req.MetricType)) return true;
                var (min, max) = HealthMetricRules.RangeOf(req.MetricType);
                return value >= min && value <= max;
            })
            .WithMessage(req =>
            {
                var (min, max) = HealthMetricRules.RangeOf(req.MetricType);
                return $"ValueNumber cho '{req.MetricType}' phai trong khoang {min} - {max}";
            });

        // Tam truong bat buoc va trong range cho blood_pressure
        RuleFor(x => x.ValueNumber2)
            .NotNull()
                .When(x => x.MetricType == "blood_pressure")
                .WithMessage("ValueNumber2 (tam truong) la bat buoc cho blood_pressure")
            .Must(v => !v.HasValue || (v.Value >= 30m && v.Value <= 200m))
                .When(x => x.MetricType == "blood_pressure")
                .WithMessage("ValueNumber2 (tam truong) phai trong khoang 30 - 200");

        RuleFor(x => x.Unit).MaximumLength(20);

        RuleFor(x => x.RecordedAt)
            .Must(d => !d.HasValue || d.Value <= DateTime.UtcNow.AddMinutes(5))
            .WithMessage("RecordedAt khong duoc o tuong lai");
    }
}

public class HealthMetricUpdateRequestValidator : AbstractValidator<HealthMetricUpdateRequest>
{
    public HealthMetricUpdateRequestValidator()
    {
        RuleFor(x => x.MetricType)
            .NotEmpty()
            .Must(v => HealthMetricRules.AllowedTypes.Contains(v))
            .WithMessage("MetricType phai la mot trong: blood_pressure, heart_rate, temperature, weight, blood_sugar, oxygen_saturation");

        RuleFor(x => x.ValueNumber)
            .Must((req, value) =>
            {
                if (!HealthMetricRules.AllowedTypes.Contains(req.MetricType)) return true;
                var (min, max) = HealthMetricRules.RangeOf(req.MetricType);
                return value >= min && value <= max;
            })
            .WithMessage(req =>
            {
                var (min, max) = HealthMetricRules.RangeOf(req.MetricType);
                return $"ValueNumber cho '{req.MetricType}' phai trong khoang {min} - {max}";
            });

        RuleFor(x => x.ValueNumber2)
            .NotNull()
                .When(x => x.MetricType == "blood_pressure")
                .WithMessage("ValueNumber2 (tam truong) la bat buoc cho blood_pressure")
            .Must(v => !v.HasValue || (v.Value >= 30m && v.Value <= 200m))
                .When(x => x.MetricType == "blood_pressure")
                .WithMessage("ValueNumber2 (tam truong) phai trong khoang 30 - 200");

        RuleFor(x => x.Unit).MaximumLength(20);

        RuleFor(x => x.RecordedAt)
            .Must(d => !d.HasValue || d.Value <= DateTime.UtcNow.AddMinutes(5))
            .WithMessage("RecordedAt khong duoc o tuong lai");
    }
}
