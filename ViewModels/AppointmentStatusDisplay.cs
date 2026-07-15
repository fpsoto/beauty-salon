using BeautySalon.Domain.Enums;

namespace Beauty_Salon.ViewModels;

// Spanish display labels/colors for AppointmentStatus - the enum itself is English
// (code identifier), but the salon owner-facing UI is Spanish.
public static class AppointmentStatusDisplay
{
    public static string ToSpanishLabel(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Booked => "Reservada",
        AppointmentStatus.Confirmed => "Confirmada",
        AppointmentStatus.InProgress => "En progreso",
        AppointmentStatus.Completed => "Finalizada",
        AppointmentStatus.Cancelled => "Cancelada",
        AppointmentStatus.NoShow => "No asistió",
        AppointmentStatus.Rescheduled => "Reagendada",
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
