using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BeautySalon.Persistence;

// Lets `dotnet ef migrations add` run directly against this project, without needing
// to build/launch the MAUI head to resolve a DbContext at design time.
public class BeautySalonDbContextFactory : IDesignTimeDbContextFactory<BeautySalonDbContext>
{
    public BeautySalonDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BeautySalonDbContext>();
        optionsBuilder.UseSqlite("Data Source=designtime.db3");

        return new BeautySalonDbContext(optionsBuilder.Options);
    }
}
