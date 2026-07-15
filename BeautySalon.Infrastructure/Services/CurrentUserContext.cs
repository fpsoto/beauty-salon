using BeautySalon.Application.Common;
using BeautySalon.Application.Common.Interfaces;

namespace BeautySalon.Infrastructure.Services;

// Stub until real authentication exists (Phase 4 login UI) - always resolves to the
// single seeded admin. Swapping this for a session-aware implementation later
// requires no change to any consumer (the audit interceptor, future AppServices).
public sealed class CurrentUserContext : ICurrentUserContext
{
    public Guid? UserId => WellKnownIds.AdminUserId;
    public string? Username => "admin";
}
