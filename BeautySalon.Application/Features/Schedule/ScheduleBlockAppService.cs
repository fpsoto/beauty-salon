using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Common;
using BeautySalon.Domain.Entities;
using FluentValidation;

namespace BeautySalon.Application.Features.Schedule;

public sealed class ScheduleBlockAppService : IScheduleBlockAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateScheduleBlockRequest> _createValidator;

    public ScheduleBlockAppService(IUnitOfWork unitOfWork, IValidator<CreateScheduleBlockRequest> createValidator)
    {
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
    }

    public async Task<Result<IReadOnlyList<ScheduleBlockDto>>> GetByWeekAsync(DateOnly anyDateInWeek, Guid professionalId, CancellationToken cancellationToken = default)
    {
        var (start, end) = WeekHelper.GetWeek(anyDateInWeek);
        var blocks = await _unitOfWork.ScheduleBlocks.GetByDateRangeAsync(start, end, professionalId, cancellationToken);

        return Result.Success<IReadOnlyList<ScheduleBlockDto>>(blocks.Select(b => b.ToDto()).ToList());
    }

    public async Task<Result<ScheduleBlockDto>> CreateAsync(CreateScheduleBlockRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<ScheduleBlockDto>(Error.Validation("ScheduleBlock.Invalid", validation.ToString(" ")));

        // Only guards against clobbering an existing appointment - a block is
        // precisely how unavailability outside/within working hours gets declared,
        // so it must not be checked against IWorkingHoursProvider itself.
        var hasAppointmentOverlap = await _unitOfWork.Appointments.HasOverlapAsync(
            request.ProfessionalId, request.Date, request.StartTime, request.EndTime, cancellationToken: cancellationToken);
        if (hasAppointmentOverlap)
            return Result.Failure<ScheduleBlockDto>(Error.Conflict("ScheduleBlock.Overlap", "Ya existe una cita en ese horario."));

        var block = new ScheduleBlock
        {
            ProfessionalId = request.ProfessionalId,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Type = request.Type,
            Reason = request.Reason
        };

        _unitOfWork.ScheduleBlocks.Add(block);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(block.ToDto());
    }

    public async Task<Result> DeleteAsync(Guid scheduleBlockId, CancellationToken cancellationToken = default)
    {
        var block = await _unitOfWork.ScheduleBlocks.GetByIdAsync(scheduleBlockId, cancellationToken);
        if (block is null)
            return Result.Failure(Error.NotFound("ScheduleBlock.NotFound", "Bloqueo no encontrado."));

        _unitOfWork.ScheduleBlocks.Remove(block);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
