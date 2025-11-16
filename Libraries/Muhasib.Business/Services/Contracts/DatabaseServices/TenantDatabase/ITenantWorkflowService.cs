using Muhasib.Business.Models.TenantModel;
using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase
{
    public interface ITenantWorkflowService
    {
        Task<ApiDataResponse<TenantCreationResult>> CreateNewTenantAsync(TenantCreationRequest request);
        Task<ApiDataResponse<bool>> DeleteTenantCompleteAsync(long maliDonemId);
        Task<ApiDataResponse<bool>> PrepareTenantForFirstUseAsync(long maliDonemId);
    }
}
