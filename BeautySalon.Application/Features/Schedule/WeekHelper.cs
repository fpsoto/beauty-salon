namespace BeautySalon.Application.Features.Schedule;

internal static class WeekHelper
{
    // ISO week: Monday through Sunday.
    public static (DateOnly Start, DateOnly End) GetWeek(DateOnly anyDateInWeek)
    {
        var diff = ((int)anyDateInWeek.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var start = anyDateInWeek.AddDays(-diff);
        return (start, start.AddDays(6));
    }
}
