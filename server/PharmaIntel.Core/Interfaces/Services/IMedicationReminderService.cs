// =============================================================================
// Interface: IMedicationReminderService
// Chuc nang: Quan ly lich nhac thuoc + log tung lan nhac (user-scoped).
// =============================================================================
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.MedicationReminders;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IMedicationReminderService
{
    Task<PagedResult<MedicationReminderListItemDto>> ListMyAsync(long userId, MedicationReminderListQuery query, CancellationToken ct = default);
    Task<MedicationReminderDto> GetByIdAsync(long userId, long id, CancellationToken ct = default);
    Task<MedicationReminderDto> CreateAsync(long userId, MedicationReminderCreateRequest request, CancellationToken ct = default);
    Task<MedicationReminderDto> UpdateAsync(long userId, long id, MedicationReminderUpdateRequest request, CancellationToken ct = default);
    Task DeleteAsync(long userId, long id, CancellationToken ct = default);

    Task<MedicationReminderLogDto> AddLogAsync(long userId, long reminderId, MedicationReminderLogCreateRequest request, CancellationToken ct = default);
    Task<PagedResult<MedicationReminderLogDto>> ListLogsAsync(long userId, long reminderId, MedicationReminderLogListQuery query, CancellationToken ct = default);
}
