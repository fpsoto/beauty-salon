using System.Linq.Expressions;
using System.Reflection;
using BeautySalon.Domain.Common;
using BeautySalon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Persistence;

public class BeautySalonDbContext : DbContext
{
    public BeautySalonDbContext(DbContextOptions<BeautySalonDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<SalonService> SalonServices => Set<SalonService>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentServiceItem> AppointmentServiceItems => Set<AppointmentServiceItem>();
    public DbSet<ScheduleBlock> ScheduleBlocks => Set<ScheduleBlock>();
    public DbSet<WorkingHours> WorkingHours => Set<WorkingHours>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();
    public DbSet<NotificationRule> NotificationRules => Set<NotificationRule>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Applied once via reflection for every AuditableEntity instead of repeating
        // the same concurrency token + soft-delete filter in all 12 configurations.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(AuditableEntity.Version))
                .IsConcurrencyToken();

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(BuildNotDeletedFilter(entityType.ClrType));
        }
    }

    private static LambdaExpression BuildNotDeletedFilter(Type entityClrType)
    {
        var parameter = Expression.Parameter(entityClrType, "entity");
        var isDeletedProperty = Expression.Property(parameter, nameof(AuditableEntity.IsDeleted));
        var notDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false));

        return Expression.Lambda(notDeleted, parameter);
    }
}
