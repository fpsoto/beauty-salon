using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Features.Settings;

public static class SettingsMappingExtensions
{
    public static AppSettingsDto ToDto(this AppSettings settings) => new(
        settings.Id,
        settings.Language,
        settings.Theme,
        settings.Currency,
        settings.DateFormat,
        settings.TimeFormat,
        settings.NotificationRules.OrderBy(r => r.MinutesBefore).Select(r => r.ToDto()).ToList());

    public static NotificationRuleDto ToDto(this NotificationRule rule) => new(rule.Id, rule.MinutesBefore, rule.IsEnabled);

    public static WorkingHoursDto ToDto(this WorkingHours hours) => new(hours.DayOfWeek, hours.StartTime, hours.EndTime, hours.IsWorkingDay);
}
