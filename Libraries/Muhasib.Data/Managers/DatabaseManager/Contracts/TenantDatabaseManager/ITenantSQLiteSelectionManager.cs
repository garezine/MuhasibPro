using Muhasib.Data.DataContext;

namespace Muhasib.Data.Managers.DatabaseManager.Contracts.TenantSqliteManager
{
    public interface ITenantSQLiteSelectionManager
    {
        Task<TenantContext> SwitchToTenantAsync(string databaseName);
        // Current State
        TenantContext GetCurrentTenant();
        Task<string> GetCurrentTenantConnectionStringAsync();
        bool IsTenantLoaded { get; }        
        // Events (optional - for UI notifications)
        event Action<TenantContext> TenantChanged;
        void ClearCurrentTenant();



    }
}
