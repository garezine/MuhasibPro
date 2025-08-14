using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Interfaces.Services;

namespace Muhasebe.Data.Database.Concreate.Services
{
    public class DatabaseBackupCleanupService : IDatabaseBackupCleanupService
    {
        private readonly ILogger<DatabaseBackupCleanupService> _logger;
        // Temizlenecek yedek dosyası uzantıları (projenize göre ayarlayın)
        private readonly string[] _supportedBackupExtensions = { ".bak", ".sqlite" }; // Örnek

        public DatabaseBackupCleanupService(ILogger<DatabaseBackupCleanupService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Belirtilen dizindeki ve alt dizinlerindeki belirli bir günden eski yedek dosyalarını siler.
        /// </summary>
        /// <param name="backupBaseDirectory">Yedeklerin bulunduğu ana dizin.</param>
        /// <param name="maxAgeInDays">Yedeklerin saklanacağı maksimum gün sayısı. Bu süreden eski yedekler silinir.</param>
        public void CleanupOldBackups(string backupBaseDirectory, int maxAgeInDays)
        {
            if (maxAgeInDays <= 0)
            {
                _logger.LogWarning("maxAgeInDays ({Days}) must be positive. Skipping cleanup.", maxAgeInDays);
                return;
            }

            if (!Directory.Exists(backupBaseDirectory))
            {
                _logger.LogWarning("Backup base directory not found: {Directory}. Skipping cleanup.", backupBaseDirectory);
                return;
            }

            _logger.LogInformation("Starting cleanup of old backups (older than {Days} days) in directory: {Directory}", maxAgeInDays, backupBaseDirectory);

            var cutoffDate = DateTime.UtcNow.AddDays(-maxAgeInDays);
            int deletedCount = 0;
            int errorCount = 0;

            try
            {
                // Desteklenen tüm uzantılar için dosyaları bul
                var filesToDelete = Directory.EnumerateFiles(backupBaseDirectory, "*.*", SearchOption.AllDirectories)
                                             .Where(file => _supportedBackupExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()));

                foreach (var file in filesToDelete)
                {
                    try
                    {
                        var lastWriteTime = File.GetLastWriteTimeUtc(file);
                        if (lastWriteTime < cutoffDate)
                        {
                            _logger.LogDebug("Attempting to delete old backup file: {File} (LastWriteTime: {LastWriteTime})", file, lastWriteTime);
                            File.Delete(file);
                            deletedCount++;
                            _logger.LogInformation("Deleted old backup: {File}", file);
                        }
                    }
                    catch (IOException ioEx) // Dosya kilitli olabilir veya yetki sorunu
                    {
                        errorCount++;
                        _logger.LogError(ioEx, "Could not delete backup file (possibly locked or permission issue): {File}", file);
                        // Hata durumunda diğer dosyaları silmeye devam et
                    }
                    catch (UnauthorizedAccessException uaEx) // Yetki sorunu
                    {
                        errorCount++;
                        _logger.LogError(uaEx, "Unauthorized access while trying to delete backup file: {File}", file);
                    }
                    catch (Exception ex) // Diğer beklenmedik hatalar
                    {
                        errorCount++;
                        _logger.LogError(ex, "Unexpected error deleting backup file: {File}", file);
                    }
                }
            }
            catch (Exception ex) // Directory.EnumerateFiles hatası vb.
            {
                _logger.LogError(ex, "Error enumerating files in backup directory: {Directory}", backupBaseDirectory);
                errorCount++; // Genel bir hata olarak sayalım
            }

            _logger.LogInformation("Backup cleanup finished. Deleted: {DeletedCount} files. Errors: {ErrorCount}.", deletedCount, errorCount);
        }
    }
}
