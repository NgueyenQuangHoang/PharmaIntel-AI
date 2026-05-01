// =============================================================================
// Validator: UpdateUserSettingsRequestValidator
// DarkMode phai khop CK_user_settings_dark_mode (light/dark/system).
// LanguageCode hien tai support: vi, en.
// =============================================================================
using FluentValidation;
using PharmaIntel.Core.DTOs.Users;

namespace PharmaIntel.Core.Validators.Users;

public class UpdateUserSettingsRequestValidator : AbstractValidator<UpdateUserSettingsRequest>
{
    private static readonly string[] AllowedDarkModes = ["light", "dark", "system"];
    private static readonly string[] AllowedLanguages = ["vi", "en"];

    public UpdateUserSettingsRequestValidator()
    {
        RuleFor(x => x.DarkMode)
            .NotEmpty().WithMessage("Che do hien thi la bat buoc")
            .Must(v => AllowedDarkModes.Contains(v))
            .WithMessage("DarkMode phai la mot trong: light, dark, system");

        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Ngon ngu la bat buoc")
            .MaximumLength(10)
            .Must(v => AllowedLanguages.Contains(v))
            .WithMessage("LanguageCode phai la mot trong: vi, en");
    }
}
