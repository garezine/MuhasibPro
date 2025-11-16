using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Data.Managers.DatabaseManager.Models;

namespace Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager
{
    public interface ITenantConnectionManager
    {
        // Saf connection management
        Task<string> GetConnectionStringAsync(long maliDonemId);
        Task<ConnectionTestResult> TestConnectionAsync(long maliDonemId);
        Task<(bool IsValid, string Message)> ValidateTenantAsync(long maliDonemId);
        Task<string> GetConnectionStringInfoAsync(long maliDonemId);   

    }
}
