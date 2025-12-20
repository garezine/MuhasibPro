using Muhasib.Data.Managers.DatabaseManager.Models;
using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase
{
    public interface ITenantSQLiteDatabaseOperationService
    {        
        Task<ApiDataResponse<bool>> CreateBackupAsync(string databaseName);
        Task<ApiDataResponse<DatabaseHealthInfo>> GetHealthStatusAsync(string databaseName);
        Task<ApiDataResponse<List<BackupFileInfo>>> GetBackupHistoryAsync(string databaseName);       
        Task<ApiDataResponse<bool>> RestoreBackupAsync(string databaseName, string backupFilePath);
        Task<ApiDataResponse<bool>> ValidateConnectionAsync(string databaseName, CancellationToken cancellationToken = default);
    }
}
