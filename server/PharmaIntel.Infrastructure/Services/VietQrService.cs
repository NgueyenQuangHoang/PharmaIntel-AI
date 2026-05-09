// =============================================================================
// Service: VietQrService
// Chuc nang: Sinh URL anh QR theo chuan VietQR (img.vietqr.io).
// Format: https://img.vietqr.io/image/{BankCode}-{AccountNo}-{Template}.png
//          ?amount={amount}&addInfo={transferContent}&accountName={accountName}
// =============================================================================
using System.Globalization;
using System.Net;
using Microsoft.Extensions.Options;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.Infrastructure.Services;

public class VietQrService : IVietQrService
{
    private readonly BankQrSettings _options;

    public VietQrService(IOptions<BankQrSettings> options)
    {
        _options = options.Value;
    }

    public string CreateQrUrl(decimal amount, string orderCode)
    {
        var transferContent = CreateTransferContent(orderCode);

        // amount truyen sang VietQR la so nguyen (don vi VND).
        var amountText = Math.Round(amount, 0).ToString("0", CultureInfo.InvariantCulture);
        var encodedAddInfo = WebUtility.UrlEncode(transferContent);
        var encodedAccountName = WebUtility.UrlEncode(_options.AccountName);

        return $"https://img.vietqr.io/image/{_options.BankCode}-{_options.AccountNo}-{_options.Template}.png" +
               $"?amount={amountText}" +
               $"&addInfo={encodedAddInfo}" +
               $"&accountName={encodedAccountName}";
    }

    public string CreateTransferContent(string orderCode) => orderCode;
}
