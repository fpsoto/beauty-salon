using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Features.Schedule;

public static class ScheduleMappingExtensions
{
    public static AppointmentServiceItemDto ToDto(this AppointmentServiceItem item) =>
        new(item.ServiceId, item.SnapshotServiceName, item.SnapshotPrice, item.SnapshotDurationMinutes, item.SortOrder);

    public static AppointmentDto ToDto(this Appointment appointment) => new(
        appointment.Id,
        appointment.ClientId,
        appointment.Client is not null ? $"{appointment.Client.Name} {appointment.Client.LastName}" : string.Empty,
        appointment.ProfessionalId,
        appointment.Date,
        appointment.StartTime,
        appointment.EndTime,
        appointment.Status,
        appointment.Notes,
        appointment.SuggestedPrice,
        appointment.ChargedPrice,
        appointment.Discount,
        appointment.Tip,
        appointment.PaymentMethodId,
        appointment.PaymentMethod?.Name,
        appointment.InternalNotes,
        appointment.RescheduledFromAppointmentId,
        appointment.ServiceItems.OrderBy(i => i.SortOrder).Select(i => i.ToDto()).ToList());

    public static ScheduleBlockDto ToDto(this ScheduleBlock block) => new(
        block.Id, block.ProfessionalId, block.Date, block.StartTime, block.EndTime, block.Type, block.Reason);
}
