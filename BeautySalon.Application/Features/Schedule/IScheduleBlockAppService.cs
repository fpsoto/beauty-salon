using BeautySalon.Domain.Common;

namespace BeautySalon.Application.Features.Schedule;

public interface IScheduleBlockAppService
{
    Task<Result<IReadOnlyList<ScheduleBlockDto>>> GetByWeekAsync(DateOnly anyDateInWeek, Guid professionalId, CancellationToken cancellationToken = default);
    Task<Result<ScheduleBlockDto>> CreateAsync(CreateScheduleBlockRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid scheduleBlockId, CancellationToken cancellationToken = default);
}
