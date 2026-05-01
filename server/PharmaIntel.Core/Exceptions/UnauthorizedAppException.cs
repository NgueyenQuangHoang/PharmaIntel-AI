// =============================================================================
// Exception: UnauthorizedAppException (401)
// Chuc nang: Sai thong tin xac thuc - sai password, token het han, tai khoan disabled.
// Dung ten "App" de tranh nham voi UnauthorizedAccessException cua framework.
// =============================================================================
namespace PharmaIntel.Core.Exceptions;

public class UnauthorizedAppException : AppException
{
    public override int StatusCode => 401;
    public override string ErrorType => "unauthorized";

    public UnauthorizedAppException(string message) : base(message) { }
}
