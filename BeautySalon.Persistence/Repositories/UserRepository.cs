using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Persistence.Repositories;

public class UserRepository : EfRepository<User>, IUserRepository
{
    public UserRepository(BeautySalonDbContext context) : base(context)
    {
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
}
