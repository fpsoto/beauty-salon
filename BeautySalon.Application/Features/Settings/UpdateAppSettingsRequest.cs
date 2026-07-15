using BeautySalon.Domain.Enums;

namespace BeautySalon.Application.Features.Settings;

public sealed record UpdateAppSettingsRequest(string Language, AppTheme Theme, Currency Currency, string DateFormat, string TimeFormat);
