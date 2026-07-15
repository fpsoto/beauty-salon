namespace BeautySalon.Application.Features.Schedule;

public sealed record RescheduleAppointmentRequest(Guid AppointmentId, DateOnly NewDate, TimeOnly NewStartTime);
