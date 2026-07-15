using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Common;
using BeautySalon.Domain.Entities;
using BeautySalon.Domain.Enums;
using FluentValidation;

namespace BeautySalon.Application.Features.Schedule;

public sealed class AppointmentAppService : IAppointmentAppService
{
    private static readonly AppointmentStatus[] TerminalStates =
    [
        AppointmentStatus.Cancelled, AppointmentStatus.Completed, AppointmentStatus.NoShow, AppointmentStatus.Rescheduled
    ];

    private readonly IUnitOfWork _unitOfWork;
    private readonly IScheduleAvailabilityChecker _availabilityChecker;
    private readonly IValidator<CreateAppointmentRequest> _createValidator;
    private readonly IValidator<RescheduleAppointmentRequest> _rescheduleValidator;
    private readonly IValidator<FinishAppointmentRequest> _finishValidator;

    public AppointmentAppService(
        IUnitOfWork unitOfWork,
        IScheduleAvailabilityChecker availabilityChecker,
        IValidator<CreateAppointmentRequest> createValidator,
        IValidator<RescheduleAppointmentRequest> rescheduleValidator,
        IValidator<FinishAppointmentRequest> finishValidator)
    {
        _unitOfWork = unitOfWork;
        _availabilityChecker = availabilityChecker;
        _createValidator = createValidator;
        _rescheduleValidator = rescheduleValidator;
        _finishValidator = finishValidator;
    }

    public async Task<Result<WeekAgendaDto>> GetWeekAsync(DateOnly anyDateInWeek, Guid professionalId, CancellationToken cancellationToken = default)
    {
        var (start, end) = WeekHelper.GetWeek(anyDateInWeek);

        var appointments = await _unitOfWork.Appointments.GetByDateRangeAsync(start, end, professionalId, cancellationToken);
        var blocks = await _unitOfWork.ScheduleBlocks.GetByDateRangeAsync(start, end, professionalId, cancellationToken);

        var dto = new WeekAgendaDto(
            start,
            end,
            appointments.Select(a => a.ToDto()).ToList(),
            blocks.Select(b => b.ToDto()).ToList());

        return Result.Success(dto);
    }

    public async Task<Result<AppointmentDto>> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<AppointmentDto>(Error.Validation("Appointment.Invalid", validation.ToString(" ")));

        var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId, cancellationToken);
        if (client is null)
            return Result.Failure<AppointmentDto>(Error.NotFound("Client.NotFound", "Cliente no encontrado."));

        var serviceItems = new List<AppointmentServiceItem>();
        var totalMinutes = 0;
        var totalPrecio = 0m;

        foreach (var serviceId in request.ServiceIds)
        {
            var service = await _unitOfWork.SalonServices.GetByIdAsync(serviceId, cancellationToken);
            if (service is null)
                return Result.Failure<AppointmentDto>(Error.NotFound("Service.NotFound", $"Servicio no encontrado: {serviceId}."));

            serviceItems.Add(new AppointmentServiceItem
            {
                ServiceId = service.Id,
                SnapshotServiceName = service.Name,
                SnapshotPrice = service.SuggestedPrice,
                SnapshotDurationMinutes = service.DurationMinutes,
                SortOrder = serviceItems.Count
            });

            totalMinutes += service.DurationMinutes;
            totalPrecio += service.SuggestedPrice;
        }

        var endTime = request.StartTime.AddMinutes(totalMinutes);

        var availability = await _availabilityChecker.EnsureAvailableAsync(
            request.ProfessionalId, request.Date, request.StartTime, endTime, cancellationToken: cancellationToken);
        if (availability.IsFailure)
            return Result.Failure<AppointmentDto>(availability.Error);

        var appointment = new Appointment
        {
            ClientId = request.ClientId,
            ProfessionalId = request.ProfessionalId,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = endTime,
            Status = AppointmentStatus.Booked,
            Notes = request.Notes,
            SuggestedPrice = totalPrecio,
            ServiceItems = serviceItems
        };

        _unitOfWork.Appointments.Add(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        appointment.Client = client;
        return Result.Success(appointment.ToDto());
    }

    public async Task<Result<AppointmentDto>> RescheduleAsync(RescheduleAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _rescheduleValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<AppointmentDto>(Error.Validation("Appointment.Invalid", validation.ToString(" ")));

        var original = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (original is null)
            return Result.Failure<AppointmentDto>(Error.NotFound("Appointment.NotFound", "Cita no encontrada."));

        if (TerminalStates.Contains(original.Status))
            return Result.Failure<AppointmentDto>(Error.Conflict("Appointment.InvalidTransition", "No se puede reagendar una cita en este status."));

        var durationMinutes = (original.EndTime.ToTimeSpan() - original.StartTime.ToTimeSpan()).TotalMinutes;
        var newEndTime = request.NewStartTime.AddMinutes(durationMinutes);

        var availability = await _availabilityChecker.EnsureAvailableAsync(
            original.ProfessionalId, request.NewDate, request.NewStartTime, newEndTime,
            excludeAppointmentId: original.Id, cancellationToken: cancellationToken);
        if (availability.IsFailure)
            return Result.Failure<AppointmentDto>(availability.Error);

        var nueva = new Appointment
        {
            ClientId = original.ClientId,
            ProfessionalId = original.ProfessionalId,
            Date = request.NewDate,
            StartTime = request.NewStartTime,
            EndTime = newEndTime,
            Status = AppointmentStatus.Booked,
            Notes = original.Notes,
            SuggestedPrice = original.SuggestedPrice,
            InternalNotes = original.InternalNotes,
            RescheduledFromAppointmentId = original.Id,
            ServiceItems = original.ServiceItems.Select((item, index) => new AppointmentServiceItem
            {
                ServiceId = item.ServiceId,
                SnapshotServiceName = item.SnapshotServiceName,
                SnapshotPrice = item.SnapshotPrice,
                SnapshotDurationMinutes = item.SnapshotDurationMinutes,
                SortOrder = index
            }).ToList()
        };

        original.Status = AppointmentStatus.Rescheduled;
        _unitOfWork.Appointments.Update(original);
        _unitOfWork.Appointments.Add(nueva);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        nueva.Client = original.Client;
        return Result.Success(nueva.ToDto());
    }

    public Task<Result> ConfirmAsync(Guid appointmentId, CancellationToken cancellationToken = default) =>
        TransitionAsync(appointmentId, [AppointmentStatus.Booked], AppointmentStatus.Confirmed, cancellationToken);

    public Task<Result> StartAsync(Guid appointmentId, CancellationToken cancellationToken = default) =>
        TransitionAsync(appointmentId, [AppointmentStatus.Booked, AppointmentStatus.Confirmed], AppointmentStatus.InProgress, cancellationToken);

    public Task<Result> CancelAsync(Guid appointmentId, string? reason = null, CancellationToken cancellationToken = default) =>
        TransitionAsync(
            appointmentId,
            [AppointmentStatus.Booked, AppointmentStatus.Confirmed, AppointmentStatus.InProgress],
            AppointmentStatus.Cancelled,
            cancellationToken,
            appointment =>
            {
                if (string.IsNullOrWhiteSpace(reason))
                    return;

                appointment.Notes = string.IsNullOrWhiteSpace(appointment.Notes)
                    ? $"Cancelled: {reason}"
                    : $"{appointment.Notes} | Cancelled: {reason}";
            });

    public Task<Result> MarkNoShowAsync(Guid appointmentId, CancellationToken cancellationToken = default) =>
        TransitionAsync(appointmentId, [AppointmentStatus.Booked, AppointmentStatus.Confirmed], AppointmentStatus.NoShow, cancellationToken);

    public async Task<Result<AppointmentDto>> FinishAsync(FinishAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _finishValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<AppointmentDto>(Error.Validation("Appointment.Invalid", validation.ToString(" ")));

        var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment is null)
            return Result.Failure<AppointmentDto>(Error.NotFound("Appointment.NotFound", "Cita no encontrada."));

        if (appointment.Status is not (AppointmentStatus.Booked or AppointmentStatus.Confirmed or AppointmentStatus.InProgress))
            return Result.Failure<AppointmentDto>(Error.Conflict("Appointment.InvalidTransition", "Solo se pueden finalizar citas reservadas, confirmadas o en progreso."));

        var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(request.PaymentMethodId, cancellationToken);
        if (paymentMethod is null)
            return Result.Failure<AppointmentDto>(Error.NotFound("PaymentMethod.NotFound", "Método de pago no encontrado."));

        appointment.Status = AppointmentStatus.Completed;
        appointment.ChargedPrice = request.ChargedPrice;
        appointment.Discount = request.Discount;
        appointment.Tip = request.Tip;
        appointment.PaymentMethodId = request.PaymentMethodId;
        if (!string.IsNullOrWhiteSpace(request.Notes))
            appointment.Notes = request.Notes;

        _unitOfWork.Appointments.Update(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        appointment.PaymentMethod = paymentMethod;
        return Result.Success(appointment.ToDto());
    }

    private async Task<Result> TransitionAsync(
        Guid appointmentId, AppointmentStatus[] allowedFrom, AppointmentStatus to,
        CancellationToken cancellationToken, Action<Appointment>? mutate = null)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment is null)
            return Result.Failure(Error.NotFound("Appointment.NotFound", "Cita no encontrada."));

        if (!allowedFrom.Contains(appointment.Status))
            return Result.Failure(Error.Conflict("Appointment.InvalidTransition", $"No se puede pasar de '{appointment.Status}' a '{to}'."));

        appointment.Status = to;
        mutate?.Invoke(appointment);

        _unitOfWork.Appointments.Update(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
