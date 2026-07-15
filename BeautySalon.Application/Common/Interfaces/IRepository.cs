using BeautySalon.Domain.Common;

namespace BeautySalon.Application.Common.Interfaces;

// Minimal generic CRUD shared by every aggregate. Aggregate-specific query methods
// live on the specific repository interfaces below, never here - keeps this
// reusable without leaking an EF Core/IQueryable shape that couldn't survive a
// swap to a REST API backend.
public interface IRepository<TEntity> where TEntity : AuditableEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(TEntity entity);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}
