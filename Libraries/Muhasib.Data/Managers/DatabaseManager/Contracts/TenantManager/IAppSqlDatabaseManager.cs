using Muhasib.Data.Managers.DatabaseManager.Models;
using Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities;

namespace Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager
{
    public interface IAppSqlDatabaseManager
    {
        Task<bool> InitializeDatabaseAsync(string databaseName);
        Task<bool> CreateManualBackupAsync(string databaseName);
        Task<bool> CreateNewDatabaseAsync(string databaseName);
        Task<bool> DeleteDatabaseAsync(string databaseName);
        Task<DatabaseHealthInfo> GetHealthInfoAsync(string databaseName);
        Task<List<BackupFileInfo>> GetBackupHistoryAsync(string databaseName);
        Task<MuhasebeVersiyon> GetCurrentMuhasebeVersionAsync(string databaseName);
        Task<bool> UpdateMuhasebeVersionAsync(string databaseName, string newVersion);
        Task<bool> RestoreDatabaseAsync(string databaseName, string backupFilePath);
    }
}
