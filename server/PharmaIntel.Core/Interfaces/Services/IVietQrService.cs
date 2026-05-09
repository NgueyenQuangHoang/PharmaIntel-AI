// =============================================================================
// Interface: IVietQrService
// Chuc nang: Sinh URL VietQR (img.vietqr.io) cho 1 don hang chuyen khoan ngan hang.
// Khi user quet QR, app ngan hang tu dien so tien + noi dung chuyen khoan.
// =============================================================================
namespace PharmaIntel.Core.Interfaces.Services;

public interface IVietQrService
{
    // Sinh URL anh QR (PNG) cho so tien + ma don hang.
    string CreateQrUrl(decimal amount, string orderCode);

    // Noi dung chuyen khoan hien tren QR (de FE hien copy-able).
    string CreateTransferContent(string orderCode);
}
