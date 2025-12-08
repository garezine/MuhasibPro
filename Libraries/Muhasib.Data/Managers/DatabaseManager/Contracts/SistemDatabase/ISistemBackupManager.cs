using Muhasib.Data.Managers.DatabaseManager.Models;

namespace Muhasib.Data.Managers.DatabaseManager.Contracts.SistemDatabase
{
    public interface ISistemBackupManager
    {
        Task<bool> CreateBackupAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Backup'tan geri yükler
        /// </summary>
        Task<bool> RestoreBackupAsync(string backupFileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Eski backup'ları temizler
        /// </summary>
        Task<int> CleanOldBackupsAsync(int keepLast = 10);

        /// <summary>
        /// Mevcut backup'ları listeler
        /// </summary>
        Task<List<BackupFileInfo>> GetBackupsAsync();

        /// <summary>
        /// Backup dosyasını doğrular
        /// </summary>
        Task<bool> IsValidBackupFileAsync(string backupFileName);
        DateTime? GetLastBackupDate();
        void CleanupSqliteWalFilesAsync(CancellationToken cancellationToken = default);
    }
}
