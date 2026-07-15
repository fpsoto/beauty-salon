namespace BeautySalon.Application.Features.Schedule;

public sealed record CreateAppointmentRequest(
    Guid ClientId,
    Guid ProfessionalId,
    DateOnly Date,
    TimeOnly StartTime,
    IReadOnlyList<Guid> ServiceIds,
    string? Notes);
