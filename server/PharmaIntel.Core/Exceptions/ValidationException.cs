// =============================================================================
// Exception: ValidationException (400)
// Chuc nang: Loi validation - chua dictionary cac field loi de tra ve client.
// =============================================================================
namespace PharmaIntel.Core.Exceptions;

public class ValidationException : AppException
{
    public override int StatusCode => 400;
    public override string ErrorType => "validation_error";

    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Du lieu khong hop le")
    {
        Errors = new Dictionary<string, string[]>(errors);
    }

    public ValidationException(string field, string error)
        : base("Du lieu khong hop le")
    {
        Errors = new Dictionary<string, string[]> { [field] = [error] };
    }
}
