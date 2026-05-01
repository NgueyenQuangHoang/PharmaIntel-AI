// =============================================================================
// Exception: ForbiddenException (403)
// Chuc nang: Co dang nhap nhung khong du quyen truy cap tai nguyen.
// =============================================================================
namespace PharmaIntel.Core.Exceptions;

public class ForbiddenException : AppException
{
    public override int StatusCode => 403;
    public override string ErrorType => "forbidden";

    public ForbiddenException(string message = "Khong du quyen truy cap") : base(message) { }
}
