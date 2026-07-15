using BeautySalon.Application.Features.Schedule;

namespace Beauty_Salon.ViewModels;

// Uniform display wrapper so the agenda's grouped CollectionView can render
// appointments and schedule blocks side by side without a DataTemplateSelector.
public sealed class AgendaEntry
{
    public required TimeOnly StartTime { get; init; }
    public required TimeOnly EndTime { get; init; }
    public required string Title { get; init; }
    public string? Subtitle { get; init; }
    public required Color AccentColor { get; init; }
    public AppointmentDto? Appointment { get; init; }
    public ScheduleBlockDto? Block { get; init; }

    public bool IsBlock => Block is not null;
    public string TimeRangeLabel => $"{StartTime:HH:mm} - {EndTime:HH:mm}";
    public string StatusLabel => Appointment is not null ? AppointmentStatusDisplay.ToLabel(Appointment.Status) : string.Empty;

    public static AgendaEntry FromAppointment(AppointmentDto appointment) => new()
    {
        StartTime = appointment.StartTime,
        EndTime = appointment.EndTime,
        Title = appointment.ClientFullName,
        Subtitle = string.Join(" + ", appointment.Services.Select(s => s.Name)),
        AccentColor = AppointmentStatusDisplay.ToColor(appointment.Status),
        Appointment = appointment
    };

    public static AgendaEntry FromBlock(ScheduleBlockDto block) => new()
    {
        StartTime = block.StartTime,
        EndTime = block.EndTime,
        Title = ScheduleBlockTypeDisplay.ToLabel(block.Type),
        Subtitle = block.Reason,
        AccentColor = Colors.SlateGray,
        Block = block
    };
}
