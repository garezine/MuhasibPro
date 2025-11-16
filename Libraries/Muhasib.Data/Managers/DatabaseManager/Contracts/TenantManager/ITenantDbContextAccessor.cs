using Muhasib.Data.DataContext;

namespace Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager
{
    public interface ITenantDbContextAccessor
    {
        AppDbContext GetCurrentTenantDbContextAsync();
        bool HasActiveTenant();
    }
}
