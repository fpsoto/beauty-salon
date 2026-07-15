namespace BeautySalon.Application.Common.Interfaces;

// Feeds CreatedBy/UpdatedBy/DeletedBy. Today it always resolves to the single seeded
// admin; once real multi-user auth exists this becomes session-aware without any
// consumer (the interceptor, AppServices) needing to change.
public interface ICurrentUserContext
{
    Guid? UserId { get; }
    string? Username { get; }
}
