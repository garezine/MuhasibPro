using Muhasib.Data.Managers.DatabaseManager.Models;
using System.Threading.Tasks;

namespace Muhasib.Data.Managers.DatabaseManager.Contracts.TenantSqliteManager
{
    public interface ITenantSQLiteBackupManager
    {
        /// <summary>
        /// Backup oluşturur
        /// </summary>
        Task<bool> CreateBackupAsync(string databaseName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Backup'tan geri yükler
        /// </summary>
        Task<bool> RestoreBackupAsync(string databaseName, string backupFileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Eski backup'ları temizler
        /// </summary>
        Task<int> CleanOldBackupsAsync(string databaseName, int keepLast = 10);

        /// <summary>
        /// Mevcut backup'ları listeler
        /// </summary>
        Task<List<BackupFileInfo>> GetBackupsAsync(string databaseName);

        /// <summary>
        /// Backup dosyasını doğrular
        /// </summary>
        Task<bool> IsValidBackupFileAsync(string backupFileName);
        DateTime? GetLastBackupDate(string databaseName);
        Task CleanupSqliteWalFilesAsync(string databaseName, CancellationToken cancellationToken = default);
    }
}
