using BeautySalon.Application.Common.Interfaces;

namespace BeautySalon.Infrastructure.Services;

// Session-aware current-user context. SignIn/SignOut (ISessionService) are called by the
// login/logout flow; UserId/Username (ICurrentUserContext) are read by AppServices and the
// audit interceptor. Registered as a singleton so the signed-in session lasts for the app
// process's lifetime - there's no cross-restart persistence by design, every launch starts
// back at LoginPage.
public sealed class CurrentUserContext : ICurrentUserContext, ISessionService
{
    public Guid? UserId { get; private set; }
    public string? Username { get; private set; }

    public void SignIn(Guid userId, string username)
    {
        UserId = userId;
        Username = username;
    }

    public void SignOut()
    {
        UserId = null;
        Username = null;
    }
}
