// =============================================================================
// Helper: OrderCodeGenerator
// Chuc nang: Sinh ma don dang ORD-yyyyMMdd-XXXXXX (6 ky tu alphanumeric upper).
// Khong gom 0/O/I/1/L de tranh nham doc khi in hoa don.
// =============================================================================
using System.Security.Cryptography;

namespace PharmaIntel.Infrastructure.Services.Helpers;

public static class OrderCodeGenerator
{
    private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    public static string Generate()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        Span<byte> buffer = stackalloc byte[6];
        RandomNumberGenerator.Fill(buffer);
        Span<char> code = stackalloc char[6];
        for (int i = 0; i < 6; i++) code[i] = Alphabet[buffer[i] % Alphabet.Length];
        return $"ORD-{date}-{new string(code)}";
    }
}
