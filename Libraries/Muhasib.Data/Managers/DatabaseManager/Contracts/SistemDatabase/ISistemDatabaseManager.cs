using Muhasib.Data.Managers.DatabaseManager.Models;
using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Data.Managers.DatabaseManager.Contracts.SistemDatabase
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
