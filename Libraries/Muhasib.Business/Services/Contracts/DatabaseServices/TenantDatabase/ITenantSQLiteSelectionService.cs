using Muhasib.Data.DataContext;
using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase
{
    public interface ITenantSQLiteSelectionService
    {
        Task<ApiDataResponse<TenantContext>> SwitchTenantAsync(string databaseName);
        Task<ApiDataResponse<bool>> DisconnectCurrentTenantAsync();
        ApiDataResponse<TenantContext> GetCurrentTenant();
        bool IsConnected { get; }
        void ClearCurrentTenant();
    }
}