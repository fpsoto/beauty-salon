namespace BeautySalon.Application.Features.Schedule;

public sealed record WeekAgendaDto(
    DateOnly WeekStart,
    DateOnly WeekEnd,
    IReadOnlyList<AppointmentDto> Appointments,
    IReadOnlyList<ScheduleBlockDto> Blocks);
