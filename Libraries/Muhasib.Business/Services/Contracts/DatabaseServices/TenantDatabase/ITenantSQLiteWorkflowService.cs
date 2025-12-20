using Muhasib.Business.Models.TenantModel;
using Muhasib.Data.DataContext;
using Muhasib.Data.Managers.DatabaseManager.Models;
using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase
{
    public interface ITenantSQLiteWorkflowService
    {
        Task<ApiDataResponse<TenantCreationResult>> CreateNewTenantAsync(TenantCreationRequest request);
        Task<ApiDataResponse<TenantDeletingResult>> DeleteTenantCompleteAsync(TenantDeletingRequest request);
        Task<ApiDataResponse<bool>> ValidateConnectionAsync(string databaseName, CancellationToken cancellationToken = default);
        Task<ApiDataResponse<TenantContext>> SwitchTenantAsync(string databaseName);
        Task<ApiDataResponse<DatabaseHealthInfo>> GetHealthStatusAsync(string databaseName);
    }
}
