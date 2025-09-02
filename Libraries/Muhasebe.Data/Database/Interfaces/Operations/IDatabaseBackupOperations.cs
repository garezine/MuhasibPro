using Muhasebe.Data.Database.Helpers;

namespace Muhasebe.Data.Database.Interfaces.Operations
{
    // Veritabanı yedekleme işlemleri
    // Veritabanı yedekleme işlemleri
    public interface IDatabaseBackupOperations
    {
        Task<BackupResult> BackupDatabaseAsync(string connectionString, string dbName, string dbPath, string backupFilePath);
    }
}
