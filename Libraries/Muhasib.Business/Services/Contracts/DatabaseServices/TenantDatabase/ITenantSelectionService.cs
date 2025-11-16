using Muhasib.Business.Models.TenantModel;
using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase
{
    public interface ITenantSelectionService
    {
        Task<ApiDataResponse<List<TenantSelectionModel>>> GetTenantsForSelectionAsync(long? firmaId = null);
        Task<ApiDataResponse<List<TenantSelectionModel>>> SearchTenantsAsync(string searchTerm);
        Task<ApiDataResponse<TenantDetailsModel>> GetTenantDetailsAsync(long maliDonemId);
        Task<ApiDataResponse<List<TenantSelectionModel>>> GetMaliDonemlerByFirmaAsync(long firmaId);
    }
}
