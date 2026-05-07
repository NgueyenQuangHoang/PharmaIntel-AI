// =============================================================================
// Extension: ConfigValidationExtensions
// Chuc nang: Kiem tra fail-fast cac cau hinh bat buoc luc startup. Neu thieu
//            secret quan trong, app dung ngay voi thong bao ro rang thay vi
//            chay duoc roi loi mo ho khi goi API.
// Cac kiem tra:
//   - ConnectionStrings:DefaultConnection bat buoc
//   - Jwt:Key bat buoc, toi thieu 32 ky tu (bao mat HMAC-SHA256)
//   - Gemini:ApiKey bat buoc khi Gemini:Enabled = true
// =============================================================================
namespace PharmaIntel.API.Extensions;

public static class ConfigValidationExtensions
{
    private const int JwtKeyMinLength = 32;

    public static void ValidateRequiredConfig(this IConfiguration config, IHostEnvironment env)
    {
        var errors = new List<string>();

        // 1) Connection string
        var conn = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(conn))
        {
            errors.Add("ConnectionStrings:DefaultConnection chua duoc cau hinh.");
        }

        // 2) JWT key
        var jwtKey = config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            errors.Add("Jwt:Key chua duoc cau hinh.");
        }
        else if (jwtKey.Length < JwtKeyMinLength)
        {
            errors.Add($"Jwt:Key qua ngan ({jwtKey.Length} ky tu) - yeu cau toi thieu {JwtKeyMinLength} ky tu.");
        }

        // 3) Gemini API key (chi check khi Enabled)
        var geminiEnabled = config.GetValue("Gemini:Enabled", true);
        if (geminiEnabled)
        {
            var geminiKey = config["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(geminiKey))
            {
                errors.Add("Gemini:ApiKey chua duoc cau hinh (Gemini:Enabled=true).");
            }
        }

        if (errors.Count == 0) return;

        var hint = env.IsDevelopment()
            ? "Dat secret bang User Secrets, vi du:\n" +
              "  cd server/PharmaIntel.API\n" +
              "  dotnet user-secrets set \"Jwt:Key\" \"<random-32+char-string>\"\n" +
              "  dotnet user-secrets set \"Gemini:ApiKey\" \"<your-gemini-key>\"\n" +
              "  dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<connection-string>\"  # tuy chon, mac dinh dung LocalDB"
            : "Dat secret bang environment variables (dau phan tach la __):\n" +
              "  ConnectionStrings__DefaultConnection=...\n" +
              "  Jwt__Key=...\n" +
              "  Gemini__ApiKey=...";

        var msg = "Cau hinh thieu hoac khong hop le:\n  - " + string.Join("\n  - ", errors)
                  + "\n\n" + hint
                  + "\n\nXem chi tiet trong server/CONFIGURATION.md.";
        throw new InvalidOperationException(msg);
    }
}
