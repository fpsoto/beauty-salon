namespace BeautySalon.Application.Features.Schedule;

public sealed record AppointmentServiceItemDto(Guid ServiceId, string Name, decimal Price, int DurationMinutes, int SortOrder);
