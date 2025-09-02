using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Helpers;
using Muhasebe.Data.Database.Interfaces.Operations;

namespace Muhasebe.Data.Database.Concreate.Providers.Sqlite
{
    // --- SQLite Silme ---
    public class SQLiteDeletionOperations : IDatabaseDeletionOperations
    {
        private readonly ILogger<SQLiteDeletionOperations> _logger;
        // Opsiyonel: private readonly IDatabaseDirectoryManager _directoryManager;

        public SQLiteDeletionOperations(ILogger<SQLiteDeletionOperations> logger)
        {
            _logger = logger;
        }

        public Task<DeletionResult> DeleteDatabaseAsync(string connectionString, string dbName, string dbDirectory, string dbPath)
        {
            _logger.LogWarning("Attempting to delete SQLite database file '{DbPath}' and related files...", dbPath);

            try
            {
                bool fileDeleted = false;

                // 1. Ana DB dosyasını sil
                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                    fileDeleted = true;
                    _logger.LogInformation("Deleted main SQLite file: {DbPath}", dbPath);
                }
                else
                {
                    _logger.LogWarning("Main SQLite file not found: {DbPath}", dbPath);
                }

                // 2. İlişkili dosyaları sil (-journal, -wal, -shm)
                string[] relatedFiles = { "-journal", "-wal", "-shm" };
                foreach (var suffix in relatedFiles)
                {
                    string filePath = dbPath + suffix;
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        _logger.LogDebug("Deleted related file: {FilePath}", filePath);
                    }
                }

                // 3. Opsiyonel: Boş dizini sil (Sadece ana dosya silindiyse)
                bool dirDeleted = false;
                if (fileDeleted && Directory.Exists(dbDirectory))
                {
                    try
                    {
                        if (!Directory.EnumerateFileSystemEntries(dbDirectory).Any())
                        {
                            Directory.Delete(dbDirectory);
                            dirDeleted = true;
                            _logger.LogInformation("Deleted empty directory: {Directory}", dbDirectory);
                        }
                        else
                        {
                            _logger.LogWarning("Directory not empty: {Directory}", dbDirectory);
                        }
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError(ex, "Directory deletion failed: {Directory}", dbDirectory);
                    }
                }

                // 4. Sonuç mesajını oluştur
                string message = fileDeleted
                    ? "SQLite database and related files deleted successfully."
                    : "No database file found to delete.";

                if (dirDeleted) message += " Directory cleaned.";

                return Task.FromResult(new DeletionResult(
                    success: fileDeleted || dirDeleted,
                    message: message,
                    dbName: dbName,
                    directoryPath: dbDirectory
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQLite deletion failed: {DbPath}", dbPath);
                return Task.FromResult(new DeletionResult(
                    success: false,
                    message: $"Deletion failed: {ex.Message}",
                    dbName: dbName,
                    directoryPath: dbDirectory
                ));
            }
        }
    }
}
