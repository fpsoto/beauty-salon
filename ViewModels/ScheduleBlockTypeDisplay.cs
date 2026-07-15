using Beauty_Salon.Resources.Strings;
using BeautySalon.Domain.Enums;

namespace Beauty_Salon.ViewModels;

public static class ScheduleBlockTypeDisplay
{
    public static string ToLabel(ScheduleBlockType type) => type switch
    {
        ScheduleBlockType.Lunch => AppResources.BlockLunch,
        ScheduleBlockType.Vacation => AppResources.BlockVacation,
        ScheduleBlockType.Meeting => AppResources.BlockMeeting,
        ScheduleBlockType.DayOff => AppResources.BlockDayOff,
        ScheduleBlockType.Unavailable => AppResources.BlockUnavailable,
        ScheduleBlockType.Other => AppResources.BlockOther,
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
