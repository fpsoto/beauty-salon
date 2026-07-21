namespace Beauty_Salon.Services;

// Persists the signed-in user across app restarts via MAUI Preferences (plain key-value,
// not SecureStorage - only a user id/username is stored, no credential/token, so the extra
// keystore complexity and occasional cross-OS-upgrade failures of SecureStorage aren't
// worth it here). Lives in the MAUI head (not Infrastructure) since Preferences is a MAUI
// Essentials API - same reasoning as WhatsApp/local notifications living here.
public interface IPersistedSessionStore
{
    void Save(Guid userId, string username);
    void Clear();

    // Returns the persisted session only if one exists and is younger than maxAge;
    // otherwise clears any stale entry and returns null.
    (Guid UserId, string Username)? TryRestore(TimeSpan maxAge);
}
