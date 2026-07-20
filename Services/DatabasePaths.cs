namespace Beauty_Salon.Services;

// Single source of truth for the SQLite file name/location, shared between MauiProgram's
// AddPersistence wiring and IDataBackupService's export/restore file operations.
public static class DatabasePaths
{
    public const string FileName = "beautysalon.db3";

    public static string FullPath => Path.Combine(FileSystem.AppDataDirectory, FileName);
}
