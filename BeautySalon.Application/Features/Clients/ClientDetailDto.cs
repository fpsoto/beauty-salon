namespace BeautySalon.Application.Features.Clients;

public sealed record ClientDetailDto(
    ClientDto Client,
    int VisitCount,
    decimal TotalSpent,
    DateOnly? LastVisit,
    DateOnly? NextAppointment,
    IReadOnlyList<AppointmentSummaryDto> Appointments);
