using Muhasib.Data.Managers.DatabaseManager.Models;
using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase
{
    public interface ITenantDatabaseOperationService
    {
        Task<ApiDataResponse<bool>> RunMigrationsAsync(long maliDonemId);
        Task<ApiDataResponse<bool>> CreateBackupAsync(long maliDonemId);
        Task<ApiDataResponse<DatabaseHealthInfo>> GetHealthStatusAsync(long maliDonemId);
        Task<ApiDataResponse<List<BackupFileInfo>>> GetBackupHistoryAsync(long maliDonemId);
        Task<ApiDataResponse<bool>> PrepareDatabaseAsync(long maliDonemId);
        Task<ApiDataResponse<bool>> RestoreBackupAsync(long maliDonemId, string backupFilePath);
    }
}
