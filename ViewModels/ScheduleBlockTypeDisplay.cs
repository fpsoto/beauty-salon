using BeautySalon.Domain.Enums;

namespace Beauty_Salon.ViewModels;

public static class ScheduleBlockTypeDisplay
{
    public static string ToSpanishLabel(ScheduleBlockType type) => type switch
    {
        ScheduleBlockType.Lunch => "Almuerzo",
        ScheduleBlockType.Vacation => "Vacaciones",
        ScheduleBlockType.Meeting => "Reunión",
        ScheduleBlockType.DayOff => "Día libre",
        ScheduleBlockType.Unavailable => "No disponible",
        ScheduleBlockType.Other => "Otro",
        _ => type.ToString()
    };

    public static readonly IReadOnlyList<ScheduleBlockType> AllTypes =
    [
        ScheduleBlockType.Lunch,
        ScheduleBlockType.Vacation,
        ScheduleBlockType.Meeting,
        ScheduleBlockType.DayOff,
        ScheduleBlockType.Unavailable,
        ScheduleBlockType.Other
    ];
}
