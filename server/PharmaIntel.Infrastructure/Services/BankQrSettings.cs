// =============================================================================
// Options: BankQrSettings
// Chuc nang: Map cau hinh "BankQr" trong appsettings.json sang object cho IOptions.
// Su dung de sinh URL VietQR (https://img.vietqr.io/image/...) cho thanh toan
// chuyen khoan ngan hang.
// =============================================================================
namespace PharmaIntel.Infrastructure.Services;

public class BankQrSettings
{
    public string BankCode { get; set; } = string.Empty;       // vd: "Vietinbank", "VCB"
    public string AccountNo { get; set; } = string.Empty;      // so tai khoan ngan hang
    public string AccountName { get; set; } = string.Empty;    // ten chu tai khoan (khong dau)
    public string Template { get; set; } = "compact2";         // template VietQR: compact, compact2, qr_only, print
}
