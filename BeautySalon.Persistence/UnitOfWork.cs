using BeautySalon.Application.Common.Exceptions;
using BeautySalon.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly BeautySalonDbContext _context;

    public UnitOfWork(
        BeautySalonDbContext context,
        IClientRepository clients,
        IAppointmentRepository appointments,
        IServiceCategoryRepository serviceCategories,
        ISalonServiceRepository salonServices,
        IScheduleBlockRepository scheduleBlocks,
        IWorkingHoursRepository workingHours,
        IPaymentMethodRepository paymentMethods,
        IUserRepository users,
        IProductRepository products,
        IAppSettingsRepository appSettings)
    {
        _context = context;
        Clients = clients;
        Appointments = appointments;
        ServiceCategories = serviceCategories;
        SalonServices = salonServices;
        ScheduleBlocks = scheduleBlocks;
        WorkingHours = workingHours;
        PaymentMethods = paymentMethods;
        Users = users;
        Products = products;
        AppSettings = appSettings;
    }

    public IClientRepository Clients { get; }
    public IAppointmentRepository Appointments { get; }
    public IServiceCategoryRepository ServiceCategories { get; }
    public ISalonServiceRepository SalonServices { get; }
    public IScheduleBlockRepository ScheduleBlocks { get; }
    public IWorkingHoursRepository WorkingHours { get; }
    public IPaymentMethodRepository PaymentMethods { get; }
    public IUserRepository Users { get; }
    public IProductRepository Products { get; }
    public IAppSettingsRepository AppSettings { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Expected business scenario (someone else edited the record), not a
            // genuine infra failure - translated to an Application-level exception
            // so no EF Core type ever escapes Persistence.
            throw new ConcurrencyConflictException(
                "El registro fue modificado por otra operación. Actualice e intente nuevamente.", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new DataAccessException("No se pudieron guardar los cambios en la base de datos.", ex);
        }
    }
}
