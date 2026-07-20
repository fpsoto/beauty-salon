using BeautySalon.Domain.Common;

namespace Beauty_Salon.Services;

public interface IDataBackupService
{
    // Copies the live database to a timestamped file under FileSystem.CacheDirectory and
    // returns its path, ready to hand to Share.Default.RequestAsync.
    Task<string> ExportBackupAsync();

    // Validates that pickedFilePath is a legitimate Beauty Salon backup, then replaces the
    // live database file with it. The app must be restarted afterward to pick up the new data.
    Task<Result> RestoreAsync(string pickedFilePath);
}
