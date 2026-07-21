using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Persistence;
using BeautySalon.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Application.Tests;

// A real SQLite database (kept alive via an open in-memory connection) rather than mocks
// or EF Core's InMemory provider - this app has already hit one real bug (Client.Rut value
// converter failing to translate under InMemory-style assumptions) that only a real
// relational provider would catch, so tests run against the same provider the app ships
// with. Each test class instantiates its own TestDatabase for full isolation between tests.
public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public BeautySalonDbContext Context { get; }
    public IUnitOfWork UnitOfWork { get; }

    public TestDatabase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<BeautySalonDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new BeautySalonDbContext(options);
        Context.Database.EnsureCreated();

        UnitOfWork = new UnitOfWork(
            Context,
            new ClientRepository(Context),
            new AppointmentRepository(Context),
            new ServiceCategoryRepository(Context),
            new SalonServiceRepository(Context),
            new ScheduleBlockRepository(Context),
            new WorkingHoursRepository(Context),
            new PaymentMethodRepository(Context),
            new UserRepository(Context),
            new ProductRepository(Context),
            new AppSettingsRepository(Context));
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
