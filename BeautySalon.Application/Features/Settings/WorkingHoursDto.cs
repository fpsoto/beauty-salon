namespace BeautySalon.Application.Features.Settings;

public sealed record WorkingHoursDto(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime, bool IsWorkingDay);
