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

    // Accent (agenda card's left bar), badge background, and badge text - each a light/dark
    // pair mirroring the Status*/Status*Container/Status*OnContainer tokens in Colors.xaml.
    // Kept as literal hex here (rather than an Application.Current.Resources lookup) since
    // this is a plain static helper with no page to resolve resources against - update both
    // places together if these ever change.
    public static Color ToAccentColor(AppointmentStatus status) => IsDarkTheme()
        ? status switch
        {
            AppointmentStatus.Booked => Color.FromArgb("#60A5FA"),
            AppointmentStatus.Confirmed => Color.FromArgb("#34D399"),
            AppointmentStatus.InProgress => Color.FromArgb("#FBBF24"),
            AppointmentStatus.Completed => Color.FromArgb("#94A3B8"),
            AppointmentStatus.Cancelled => Color.FromArgb("#F87171"),
            AppointmentStatus.NoShow => Color.FromArgb("#FDA4AF"),
            AppointmentStatus.Rescheduled => Color.FromArgb("#C4B5FD"),
            _ => Color.FromArgb("#94A3B8")
        }
        : status switch
        {
            AppointmentStatus.Booked => Color.FromArgb("#2563EB"),
            AppointmentStatus.Confirmed => Color.FromArgb("#047857"),
            AppointmentStatus.InProgress => Color.FromArgb("#B45309"),
            AppointmentStatus.Completed => Color.FromArgb("#475569"),
            AppointmentStatus.Cancelled => Color.FromArgb("#DC2626"),
            AppointmentStatus.NoShow => Color.FromArgb("#7F1D1D"),
            AppointmentStatus.Rescheduled => Color.FromArgb("#7C3AED"),
            _ => Color.FromArgb("#475569")
        };

    public static Color ToBadgeBackgroundColor(AppointmentStatus status) => IsDarkTheme()
        ? status switch
        {
            AppointmentStatus.Booked => Color.FromArgb("#172554"),
            AppointmentStatus.Confirmed => Color.FromArgb("#064E3B"),
            AppointmentStatus.InProgress => Color.FromArgb("#78350F"),
            AppointmentStatus.Completed => Color.FromArgb("#334155"),
            AppointmentStatus.Cancelled => Color.FromArgb("#7F1D1D"),
            AppointmentStatus.NoShow => Color.FromArgb("#881337"),
            AppointmentStatus.Rescheduled => Color.FromArgb("#4C1D95"),
            _ => Color.FromArgb("#334155")
        }
        : status switch
        {
            AppointmentStatus.Booked => Color.FromArgb("#DBEAFE"),
            AppointmentStatus.Confirmed => Color.FromArgb("#D1FAE5"),
            AppointmentStatus.InProgress => Color.FromArgb("#FEF3C7"),
            AppointmentStatus.Completed => Color.FromArgb("#E2E8F0"),
            AppointmentStatus.Cancelled => Color.FromArgb("#FEE2E2"),
            AppointmentStatus.NoShow => Color.FromArgb("#FECACA"),
            AppointmentStatus.Rescheduled => Color.FromArgb("#EDE9FE"),
            _ => Color.FromArgb("#E2E8F0")
        };

    public static Color ToBadgeTextColor(AppointmentStatus status) => IsDarkTheme()
        ? status switch
        {
            AppointmentStatus.Booked => Color.FromArgb("#BFDBFE"),
            AppointmentStatus.Confirmed => Color.FromArgb("#A7F3D0"),
            AppointmentStatus.InProgress => Color.FromArgb("#FDE68A"),
            AppointmentStatus.Completed => Color.FromArgb("#E2E8F0"),
            AppointmentStatus.Cancelled => Color.FromArgb("#FECACA"),
            AppointmentStatus.NoShow => Color.FromArgb("#FECDD3"),
            AppointmentStatus.Rescheduled => Color.FromArgb("#EDE9FE"),
            _ => Color.FromArgb("#E2E8F0")
        }
        : status switch
        {
            AppointmentStatus.Booked => Color.FromArgb("#1E3A8A"),
            AppointmentStatus.Confirmed => Color.FromArgb("#065F46"),
            AppointmentStatus.InProgress => Color.FromArgb("#92400E"),
            AppointmentStatus.Completed => Color.FromArgb("#334155"),
            AppointmentStatus.Cancelled => Color.FromArgb("#991B1B"),
            AppointmentStatus.NoShow => Color.FromArgb("#7F1D1D"),
            AppointmentStatus.Rescheduled => Color.FromArgb("#5B21B6"),
            _ => Color.FromArgb("#1E293B")
        };

    private static bool IsDarkTheme() => Application.Current?.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark;
}
