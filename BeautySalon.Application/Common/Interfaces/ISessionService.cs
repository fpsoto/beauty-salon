namespace BeautySalon.Application.Common.Interfaces;

// Write-side companion to ICurrentUserContext, implemented by the same class in
// Infrastructure. Kept as a separate interface so AppServices and the audit interceptor
// only ever see the read-only ICurrentUserContext - only the login/logout flow can mutate it.
public interface ISessionService
{
    void SignIn(Guid userId, string username);
    void SignOut();
}
