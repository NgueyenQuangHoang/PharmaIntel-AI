// =============================================================================
// Service: SymptomService
// Chuc nang: Doc danh muc trieu chung (public catalog).
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Diagnostics;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class SymptomService : ISymptomService
{
    private readonly PharmaIntelDbContext _db;

    public SymptomService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<List<SymptomDto>> ListAsync(string? groupName, CancellationToken ct = default)
    {
        var query = _db.Symptoms.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(groupName))
            query = query.Where(s => s.GroupName == groupName);

        return await query
            .OrderBy(s => s.GroupName)
            .ThenBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .Select(s => new SymptomDto
            {
                Id = s.Id,
                Name = s.Name,
                GroupName = s.GroupName,
                DisplayOrder = s.DisplayOrder
            })
            .ToListAsync(ct);
    }
}
