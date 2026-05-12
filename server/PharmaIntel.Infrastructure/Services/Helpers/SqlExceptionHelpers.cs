// =============================================================================
// Helper: SqlExceptionHelpers
// Chuc nang: Detect SQL Server unique constraint violation tu DbUpdateException.
// Quy tac: Code 2601 = duplicate key row, 2627 = unique constraint violation.
// =============================================================================
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace PharmaIntel.Infrastructure.Services.Helpers;

internal static class SqlExceptionHelpers
{
    public static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is SqlException sqlEx
           && (sqlEx.Number == 2601 || sqlEx.Number == 2627);

    public static bool IsUniqueViolation(DbUpdateException ex, string indexName)
        => ex.InnerException is SqlException sqlEx
           && (sqlEx.Number == 2601 || sqlEx.Number == 2627)
           && sqlEx.Message.Contains(indexName, StringComparison.OrdinalIgnoreCase);
}
