namespace Beauty_Salon.Services;

public sealed class PersistedSessionStore : IPersistedSessionStore
{
    private const string UserIdKey = "session_user_id";
    private const string UsernameKey = "session_username";
    private const string SignedInAtUtcTicksKey = "session_signed_in_at_utc_ticks";

    public void Save(Guid userId, string username)
    {
        Preferences.Default.Set(UserIdKey, userId.ToString());
        Preferences.Default.Set(UsernameKey, username);
        Preferences.Default.Set(SignedInAtUtcTicksKey, DateTime.UtcNow.Ticks);
    }

    public void Clear()
    {
        Preferences.Default.Remove(UserIdKey);
        Preferences.Default.Remove(UsernameKey);
        Preferences.Default.Remove(SignedInAtUtcTicksKey);
    }

    public (Guid UserId, string Username)? TryRestore(TimeSpan maxAge)
    {
        var userIdText = Preferences.Default.Get(UserIdKey, string.Empty);
        var username = Preferences.Default.Get(UsernameKey, string.Empty);
        var signedInAtUtcTicks = Preferences.Default.Get(SignedInAtUtcTicksKey, 0L);

        if (string.IsNullOrEmpty(userIdText) || string.IsNullOrEmpty(username) || signedInAtUtcTicks == 0
            || !Guid.TryParse(userIdText, out var userId))
        {
            Clear();
            return null;
        }

        var signedInAtUtc = new DateTime(signedInAtUtcTicks, DateTimeKind.Utc);
        if (DateTime.UtcNow - signedInAtUtc > maxAge)
        {
            Clear();
            return null;
        }

        return (userId, username);
    }
}
