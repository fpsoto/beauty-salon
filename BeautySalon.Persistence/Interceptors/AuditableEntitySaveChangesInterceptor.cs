using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BeautySalon.Persistence.Interceptors;

// Centralizes audit stamping + soft delete in one place instead of repeating it in
// every repository: Added entries get Created(At/By), Modified entries get
// Updated(At/By) plus a bumped Version, and Deleted entries are converted to a
// Modified soft-delete instead of a physical DELETE.
public sealed class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserContext _currentUserContext;

    public AuditableEntitySaveChangesInterceptor(IDateTimeProvider dateTimeProvider, ICurrentUserContext currentUserContext)
    {
        _dateTimeProvider = dateTimeProvider;
        _currentUserContext = currentUserContext;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAuditRules(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyAuditRules(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditRules(DbContext? context)
    {
        if (context is null)
            return;

        var now = _dateTimeProvider.UtcNow;
        var username = _currentUserContext.Username;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = username;
                    entry.Entity.Version = 1;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = username;
                    entry.Entity.Version += 1;
                    break;

                case EntityState.Deleted:
                    // Soft delete: keep the row, flip its flags and let EF Core persist
                    // it as an UPDATE instead of a physical DELETE.
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    entry.Entity.DeletedBy = username;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = username;
                    entry.Entity.Version += 1;
                    break;
            }
        }
    }
}
