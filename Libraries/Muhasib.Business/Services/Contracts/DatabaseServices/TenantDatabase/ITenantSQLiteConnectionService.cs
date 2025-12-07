using Muhasib.Data.DataContext;
using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase
{
    public interface ITenantSQLiteConnectionService
    {
        Task<ApiDataResponse<TenantContext>> SwitchTenantAsync(string databaseName);
        Task<ApiDataResponse<bool>> ValidateConnectionAsync(string databaseName);
        Task<ApiDataResponse<string>> TestConnectionAsync(string databaseName);
        ApiDataResponse<TenantContext> GetCurrentTenant();
        Task<ApiDataResponse<bool>> DisconnectCurrentTenantAsync();
        Task<ApiDataResponse<string>> GetConnectionInfoAsync(string databaseName);
        public void ClearCurrentTenant();
        bool IsConnected { get; }
    }
}
