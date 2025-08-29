using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Helpers;
using Muhasebe.Data.Database.Interfaces.Operations;

namespace Muhasebe.Data.Database.Concreate.Providers.Sqlite
{
    // --- SQLite Geri Yükleme ---
    public class SQLiteRestoreOperations : IDatabaseRestoreOperations
    {
        private readonly ILogger<SQLiteRestoreOperations> _logger;

        public SQLiteRestoreOperations(ILogger<SQLiteRestoreOperations> logger)
        {
            _logger = logger;
        }

        public Task<RestoreResult> RestoreDatabaseAsync(string backupFilePath, string targetDatabaseName, string targetDbDirectory, string targetDbPath)
        {
            _logger.LogInformation("Starting SQLite restore from file '{BackupFilePath}' to '{TargetDbPath}'...", backupFilePath, targetDbPath);

            if (!File.Exists(backupFilePath))
            {
                _logger.LogError("Backup file not found: {BackupFilePath}", backupFilePath);
                return Task.FromResult(new RestoreResult(false, "Yedek dosyası bulunamadı."));
            }

            try
            {
                // Hedef dizinin var olduğundan emin ol
                if (!Directory.Exists(targetDbDirectory))
                {
                    _logger.LogInformation("Target directory does not exist. Creating: {Directory}", targetDbDirectory);
                    Directory.CreateDirectory(targetDbDirectory);
                }

                // Hedefte var olan DB ve ilişkili dosyaları sil (üzerine yazmadan önce)
                if (File.Exists(targetDbPath)) File.Delete(targetDbPath);
                if (File.Exists(targetDbPath + "-journal")) File.Delete(targetDbPath + "-journal");
                if (File.Exists(targetDbPath + "-wal")) File.Delete(targetDbPath + "-wal");
                if (File.Exists(targetDbPath + "-shm")) File.Delete(targetDbPath + "-shm");
                _logger.LogDebug("Deleted existing target files (if any) at: {TargetDbPath}", targetDbPath);


                // Yedek dosyasını hedefe kopyala
                File.Copy(backupFilePath, targetDbPath, true); // overwrite = true (gerçi yukarıda sildik ama garanti)

                _logger.LogInformation("SQLite restore completed successfully to '{TargetDbPath}'.", targetDbPath);
                return Task.FromResult(new RestoreResult(true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQLite restore failed from '{BackupFilePath}' to '{TargetDbPath}'.", backupFilePath, targetDbPath);
                // Başarısız olursa kısmen kopyalanan dosyayı silmek iyi olabilir
                try { if (File.Exists(targetDbPath)) File.Delete(targetDbPath); } catch { /* Ignore cleanup error */ }
                return Task.FromResult(new RestoreResult(false, $"SQLite geri yükleme hatası: {ex.Message}", targetDatabaseName, targetDbDirectory, targetDbPath));
            }
        }
    }
}

