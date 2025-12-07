using Muhasib.Business.Models.TenantModel;
using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase
{
    public interface ITenantSQLiteWorkflowService
    {
        Task<ApiDataResponse<TenantCreationResult>> CreateNewTenantAsync(TenantCreationRequest request);
        Task<ApiDataResponse<TenantDeletingResult>> DeleteTenantCompleteAsync(TenantDeletingRequest request);
        Task<ApiDataResponse<bool>> PrepareTenantForFirstUseAsync(string databaseName);       
    }
}
