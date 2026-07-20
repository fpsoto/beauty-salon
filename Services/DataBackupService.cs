using Beauty_Salon.Resources.Strings;
using BeautySalon.Domain.Common;
using Microsoft.Data.Sqlite;

namespace Beauty_Salon.Services;

public sealed class DataBackupService : IDataBackupService
{
    public async Task<string> ExportBackupAsync()
    {
        await CheckpointAsync(DatabasePaths.FullPath);

        var exportPath = Path.Combine(FileSystem.CacheDirectory, $"beautysalon-backup-{DateTime.Now:yyyy-MM-dd-HHmm}.db3");
        File.Copy(DatabasePaths.FullPath, exportPath, overwrite: true);
        return exportPath;
    }

    public async Task<Result> RestoreAsync(string pickedFilePath)
    {
        if (!await IsValidBackupAsync(pickedFilePath))
            return Result.Failure(Error.Validation("Backup.Invalid", AppResources.InvalidBackupMessage));

        await CheckpointAsync(DatabasePaths.FullPath);

        File.Copy(pickedFilePath, DatabasePaths.FullPath, overwrite: true);

        // Drop any stale WAL/SHM sidecar files so the next app launch reads a clean single file.
        var walPath = DatabasePaths.FullPath + "-wal";
        var shmPath = DatabasePaths.FullPath + "-shm";
        if (File.Exists(walPath))
            File.Delete(walPath);
        if (File.Exists(shmPath))
            File.Delete(shmPath);

        return Result.Success();
    }

    private static async Task<bool> IsValidBackupAsync(string filePath)
    {
        try
        {
            await using var connection = new SqliteConnection($"Data Source={filePath};Mode=ReadOnly");
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Users';";
            var result = await command.ExecuteScalarAsync();
            return result is long count && count > 0;
        }
        catch
        {
            return false;
        }
    }

    // Merges any pending write-ahead-log data into the main file so a plain file copy captures
    // a complete, consistent snapshot instead of missing recent writes still sitting in -wal.
    private static async Task CheckpointAsync(string databasePath)
    {
        try
        {
            await using var connection = new SqliteConnection($"Data Source={databasePath}");
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
            await command.ExecuteNonQueryAsync();
        }
        catch
        {
            // Best-effort - if this fails, the copy just includes whatever's already in the main file.
        }
    }
}
