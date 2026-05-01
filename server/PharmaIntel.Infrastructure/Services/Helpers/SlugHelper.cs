// =============================================================================
// Helper: SlugHelper
// Chuc nang: Sinh slug ASCII tu chuoi tieng Viet (loai bo dau, replace space).
// =============================================================================
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PharmaIntel.Infrastructure.Services.Helpers;

public static class SlugHelper
{
    public static string ToSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in normalized)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (cat != UnicodeCategory.NonSpacingMark) sb.Append(ch);
        }
        var s = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        s = s.Replace('đ', 'd');
        s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
        s = Regex.Replace(s, @"\s+", "-").Trim('-');
        s = Regex.Replace(s, @"-{2,}", "-");
        return s;
    }
}
