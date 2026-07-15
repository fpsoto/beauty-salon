using BeautySalon.Domain.Common;

namespace BeautySalon.Application.Features.Schedule;

public interface IAppointmentAppService
{
    Task<Result<WeekAgendaDto>> GetWeekAsync(DateOnly anyDateInWeek, Guid professionalId, CancellationToken cancellationToken = default);
    Task<Result<AppointmentDto>> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<Result<AppointmentDto>> RescheduleAsync(RescheduleAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<Result> ConfirmAsync(Guid appointmentId, CancellationToken cancellationToken = default);
    Task<Result> StartAsync(Guid appointmentId, CancellationToken cancellationToken = default);
    Task<Result> CancelAsync(Guid appointmentId, string? reason = null, CancellationToken cancellationToken = default);
    Task<Result> MarkNoShowAsync(Guid appointmentId, CancellationToken cancellationToken = default);
    Task<Result<AppointmentDto>> FinishAsync(FinishAppointmentRequest request, CancellationToken cancellationToken = default);
}
