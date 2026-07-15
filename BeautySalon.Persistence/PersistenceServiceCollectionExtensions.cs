using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Persistence.Interceptors;
using BeautySalon.Persistence.Repositories;
using BeautySalon.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BeautySalon.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<AuditableEntitySaveChangesInterceptor>();

        services.AddDbContext<BeautySalonDbContext>((serviceProvider, options) =>
        {
            options.UseSqlite(connectionString);
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
        });

        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
        services.AddScoped<ISalonServiceRepository, SalonServiceRepository>();
        services.AddScoped<IScheduleBlockRepository, ScheduleBlockRepository>();
        services.AddScoped<IWorkingHoursRepository, WorkingHoursRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<DatabaseSeeder>();

        return services;
    }

    // Applies pending migrations and runs the seeder - called once from the
    // composition root (MauiProgram.cs) right after the app builds.
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<BeautySalonDbContext>();

        // WAL mode is persisted in the db file itself, so setting it once here is enough;
        // reduces "database is locked" errors from concurrent reads/writes (e.g. a
        // background notification check running while the UI saves an appointment).
        await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;", cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);

        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}
