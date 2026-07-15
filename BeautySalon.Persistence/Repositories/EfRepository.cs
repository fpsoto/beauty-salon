using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Persistence.Repositories;

public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : AuditableEntity
{
    protected readonly BeautySalonDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public EfRepository(BeautySalonDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.ToListAsync(cancellationToken);

    public void Add(TEntity entity) => DbSet.Add(entity);

    public void Update(TEntity entity) => DbSet.Update(entity);

    public void Remove(TEntity entity) => DbSet.Remove(entity);
}
