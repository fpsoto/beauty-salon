using BeautySalon.Domain.Enums;

namespace BeautySalon.Application.Features.Clients;

public sealed record AppointmentSummaryDto(
    Guid Id,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    AppointmentStatus Status,
    decimal? ChargedPrice,
    string ServicesSummary);
