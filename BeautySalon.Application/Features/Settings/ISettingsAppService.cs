using BeautySalon.Domain.Common;

namespace BeautySalon.Application.Features.Settings;

public interface ISettingsAppService
{
    Task<Result<AppSettingsDto>> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<Result<AppSettingsDto>> UpdateSettingsAsync(UpdateAppSettingsRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateNotificationRulesAsync(IReadOnlyList<NotificationRuleInput> rules, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<WorkingHoursDto>>> GetWorkingHoursAsync(Guid professionalId, CancellationToken cancellationToken = default);
    Task<Result> UpdateWorkingHoursAsync(Guid professionalId, UpdateWorkingHoursRequest request, CancellationToken cancellationToken = default);
}
