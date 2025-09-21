using Muhasebe.Data.DatabaseManager.Models;

namespace Muhasebe.Data.DatabaseManager.SistemDatabase
{
    public interface ISistemDatabaseManager
    {
        Task<bool> InitializeDatabaseAsync();
        Task<bool> ValidateDatabaseAsync();
        Task<bool> CreateManualBackupAsync();
        Task<bool> IsFirstRunAsync();
        Task<DatabaseHealthInfo> GetHealthInfoAsync();
        Task<List<BackupFileInfo>> GetBackupHistoryAsync();
    }
}
