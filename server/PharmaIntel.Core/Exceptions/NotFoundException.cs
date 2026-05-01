// =============================================================================
// Exception: NotFoundException (404)
// Chuc nang: Khong tim thay tai nguyen - vi du user, medication, order.
// =============================================================================
namespace PharmaIntel.Core.Exceptions;

public class NotFoundException : AppException
{
    public override int StatusCode => 404;
    public override string ErrorType => "not_found";

    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string resource, object id)
        : base($"Khong tim thay {resource} voi id = {id}") { }
}
