using BeautySalon.Domain.Enums;

namespace BeautySalon.Application.Features.Schedule;

public sealed record CreateScheduleBlockRequest(
    Guid ProfessionalId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    ScheduleBlockType Type,
    string? Reason);
