namespace BeautySalon.Application.Common;

// Fixed identity for the single seeded admin account, shared between the database
// seeder (Persistence) and the current-user stub (Infrastructure) until real
// multi-user authentication exists (Phase 4 login UI, then future multi-professional).
public static class WellKnownIds
{
    public static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
}
