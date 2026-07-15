using BeautySalon.Domain.Enums;

namespace BeautySalon.Application.Features.Settings;

public sealed record AppSettingsDto(
    Guid Id,
    string Language,
    AppTheme Theme,
    Currency Currency,
    string DateFormat,
    string TimeFormat,
    IReadOnlyList<NotificationRuleDto> NotificationRules);
