// =============================================================================
// Exception: ConflictException (409)
// Chuc nang: Trang thai xung dot - vi du email da ton tai, slug trung lap.
// =============================================================================
namespace PharmaIntel.Core.Exceptions;

public class ConflictException : AppException
{
    public override int StatusCode => 409;
    public override string ErrorType => "conflict";

    public ConflictException(string message) : base(message) { }
}
