using Muhasebe.Data.DatabaseManager.Models;
using Muhasebe.Domain.Entities.SistemEntity;

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
        Task<bool> CheckSistemDatabaseConnectionAsync();
        Task<AppVersiyon> GetCurrentAppVersionAsync();
        Task<SistemDbVersiyon> GetCurrentSistemDbVersionAsync();
    }
}
