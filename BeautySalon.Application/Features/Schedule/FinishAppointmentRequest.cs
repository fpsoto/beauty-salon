namespace BeautySalon.Application.Features.Schedule;

public sealed record FinishAppointmentRequest(
    Guid AppointmentId,
    decimal ChargedPrice,
    decimal? Discount,
    decimal? Tip,
    Guid PaymentMethodId,
    string? Notes);
