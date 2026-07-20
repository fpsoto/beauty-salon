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

    // Matches the Colors.xaml Status* design tokens - kept as literal hex here (rather than a
    // resource lookup) since this is a plain static helper with no page/BindingContext to
    // resolve Application.Current.Resources against.
    public static Color ToColor(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Booked => Color.FromArgb("#3B82F6"),
        AppointmentStatus.Confirmed => Color.FromArgb("#10B981"),
        AppointmentStatus.InProgress => Color.FromArgb("#F59E0B"),
        AppointmentStatus.Completed => Color.FromArgb("#64748B"),
        AppointmentStatus.Cancelled => Color.FromArgb("#EF4444"),
        AppointmentStatus.NoShow => Color.FromArgb("#991B1B"),
        AppointmentStatus.Rescheduled => Color.FromArgb("#8B5CF6"),
        _ => Color.FromArgb("#64748B")
    };
}
