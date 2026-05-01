// =============================================================================
// Exception: AppException (base)
// Chuc nang: Lop co so cho cac exception nghiep vu - co status code de mapper xu ly.
// Cac lop con: ValidationException, ConflictException, UnauthorizedAppException,
//              NotFoundException, ForbiddenException.
// =============================================================================
namespace PharmaIntel.Core.Exceptions;

public abstract class AppException : Exception
{
    public abstract int StatusCode { get; }
    public abstract string ErrorType { get; }

    protected AppException(string message) : base(message) { }
    protected AppException(string message, Exception inner) : base(message, inner) { }
}
