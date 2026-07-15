namespace BeautySalon.Application.Features.Settings;

public sealed record NotificationRuleDto(Guid Id, int MinutesBefore, bool IsEnabled);
