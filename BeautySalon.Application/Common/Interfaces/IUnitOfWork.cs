namespace BeautySalon.Application.Common.Interfaces;

// Aggregates every repository behind one SaveChangesAsync so a full use case commits
// atomically. AppServices inject only IUnitOfWork - never a loose repository or the
// DbContext directly.
public interface IUnitOfWork
{
    IClientRepository Clients { get; }
    IAppointmentRepository Appointments { get; }
    IServiceCategoryRepository ServiceCategories { get; }
    ISalonServiceRepository SalonServices { get; }
    IScheduleBlockRepository ScheduleBlocks { get; }
    IWorkingHoursRepository WorkingHours { get; }
    IPaymentMethodRepository PaymentMethods { get; }
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    IAppSettingsRepository AppSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
