using Muhasib.Data.DataContext;

namespace Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager
{
    public interface ITenantSelectionManager
    {
        // Tenant Operations
        Task<TenantContext> SwitchToTenantAsync(long maliDonemId);
        Task<bool> ValidateTenantAsync(long maliDonemId);

        // Current State
        TenantContext GetCurrentTenant();
        bool IsTenantLoaded { get; }

        // Events (optional - for UI notifications)
        event Action<TenantContext> TenantChanged;
    }
}
