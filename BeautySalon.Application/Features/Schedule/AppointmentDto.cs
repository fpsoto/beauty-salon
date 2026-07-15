using BeautySalon.Domain.Enums;

namespace BeautySalon.Application.Features.Schedule;

public sealed record AppointmentDto(
    Guid Id,
    Guid ClientId,
    string ClientFullName,
    Guid ProfessionalId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    AppointmentStatus Status,
    string? Notes,
    decimal SuggestedPrice,
    decimal? ChargedPrice,
    decimal? Discount,
    decimal? Tip,
    Guid? PaymentMethodId,
    string? PaymentMethodName,
    string? InternalNotes,
    Guid? RescheduledFromAppointmentId,
    IReadOnlyList<AppointmentServiceItemDto> Services);
