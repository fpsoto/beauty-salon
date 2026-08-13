namespace Beauty_Salon.Services;

// Temporary DEBUG-only helper (see SettingsPage/SettingsViewModel) that populates the
// running app with realistic-looking demo data - clients, appointments, products, sales -
// for a one-off LinkedIn demo recording. Not a permanent feature: delete this file plus
// its wiring in SettingsPage/SettingsViewModel/MauiProgram once the demo has been recorded.
public interface IDemoDataSeeder
{
    // Guarded to be a safe no-op if clients already exist (real usage or a prior seed run).
    // Returns a human-readable summary of what was created, for display in a DisplayAlert.
    Task<string> SeedAsync();
}
