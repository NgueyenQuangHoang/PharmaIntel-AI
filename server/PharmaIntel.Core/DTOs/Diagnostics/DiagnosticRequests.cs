// =============================================================================
// DTOs: Create session, add message, list query.
// =============================================================================
using PharmaIntel.Core.DTOs.Common;

namespace PharmaIntel.Core.DTOs.Diagnostics;

public class CreateDiagnosticSessionRequest
{
    public List<long> SymptomIds { get; set; } = [];           // bat buoc >= 1
    public string? InitialMessage { get; set; }                 // optional - mo ta them tu user
}

public class AddDiagnosticMessageRequest
{
    public string Content { get; set; } = string.Empty;
}

public class DiagnosticSessionListQuery : PagedQuery
{
    public string? Status { get; set; }                         // filter theo status
}
