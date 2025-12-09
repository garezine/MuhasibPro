using Muhasib.Data.Managers.DatabaseManager.Models;
using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Contracts.DatabaseServices.SistemDatabase
{
    public interface ISistemDatabaseOperationService
    {
        Task<ApiDataResponse<bool>> ValidateConnectionAsync();       
        Task<ApiDataResponse<DatabaseHealthInfo>> GetHealthStatusAsync();
        Task<ApiDataResponse<bool>> CreateBackupAsync();
        Task<ApiDataResponse<bool>> RestoreBackupAsync(string backupFilePath);
        Task<ApiDataResponse<List<BackupFileInfo>>> GetBackupHistoryAsync();
    }
}
