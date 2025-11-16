using Muhasib.Data.DataContext;
using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase
{
    public interface ITenantConnectionService
    {
        Task<ApiDataResponse<TenantContext>> SwitchTenantAsync(long maliDonemId);
        Task<ApiDataResponse<bool>> ValidateConnectionAsync(long maliDonemId);
        Task<ApiDataResponse<string>> TestConnectionAsync(long maliDonemId);
        ApiDataResponse<TenantContext> GetCurrentTenant();
        Task<ApiDataResponse<bool>> DisconnectCurrentTenantAsync();
        Task<ApiDataResponse<string>> GetConnectionInfoAsync(long maliDonemId);
    }
}
