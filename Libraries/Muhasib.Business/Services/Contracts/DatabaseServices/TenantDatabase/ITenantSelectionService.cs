using Muhasib.Business.Models.TenantModel;
using Muhasib.Data.Common;
using Muhasib.Data.Utilities.Responses;
using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase
{
    public interface ITenantSelectionService
    {
        Task<ApiDataResponse<List<TenantSelectionModel>>> GetTenantsForSelectionAsync(long? firmaId = null,DataRequest<MaliDonem> request = null);
        Task<ApiDataResponse<List<TenantSelectionModel>>> SearchTenantsAsync(string searchTerm);
        Task<ApiDataResponse<TenantDetailsModel>> GetTenantDetailsAsync(long maliDonemId);
        
    }
}
