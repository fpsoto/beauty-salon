using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Features.Clients;

public static class ClientMappingExtensions
{
    public static ClientDto ToDto(this Client client) => new(
        client.Id,
        client.Name,
        client.LastName,
        client.Rut.Value,
        client.Phone,
        client.Email,
        client.DateOfBirth,
        client.Address,
        client.Notes,
        client.IsActive,
        client.IsFavorite);

    public static AppointmentSummaryDto ToSummaryDto(this Appointment appointment) => new(
        appointment.Id,
        appointment.Date,
        appointment.StartTime,
        appointment.EndTime,
        appointment.Status,
        appointment.ChargedPrice,
        string.Join(" + ", appointment.ServiceItems.OrderBy(i => i.SortOrder).Select(i => i.SnapshotServiceName)));
}
