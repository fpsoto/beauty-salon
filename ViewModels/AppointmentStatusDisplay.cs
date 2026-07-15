using Beauty_Salon.Resources.Strings;
using BeautySalon.Domain.Enums;

namespace Beauty_Salon.ViewModels;

// Localized display labels/colors for AppointmentStatus - the enum itself is English
// (code identifier), but the salon owner-facing UI is localized via AppResources.
public static class AppointmentStatusDisplay
{
    public static string ToLabel(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Booked => AppResources.StatusBooked,
        AppointmentStatus.Confirmed => AppResources.StatusConfirmed,
        AppointmentStatus.InProgress => AppResources.StatusInProgress,
        AppointmentStatus.Completed => AppResources.StatusCompleted,
        AppointmentStatus.Cancelled => AppResources.StatusCancelled,
        AppointmentStatus.NoShow => AppResources.NoShowAction,
        AppointmentStatus.Rescheduled => AppResources.RescheduleAction,
        _ => status.ToString()
    };

    public static Color ToColor(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Booked => Colors.SteelBlue,
        AppointmentStatus.Confirmed => Colors.SeaGreen,
        AppointmentStatus.InProgress => Colors.Orange,
        AppointmentStatus.Completed => Colors.Gray,
        AppointmentStatus.Cancelled => Colors.IndianRed,
        AppointmentStatus.NoShow => Colors.DarkRed,
        AppointmentStatus.Rescheduled => Colors.MediumPurple,
        _ => Colors.Gray
    };
}
