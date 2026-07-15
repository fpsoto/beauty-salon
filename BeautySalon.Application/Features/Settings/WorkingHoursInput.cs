namespace BeautySalon.Application.Features.Settings;

public sealed record WorkingHoursInput(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime, bool IsWorkingDay);
