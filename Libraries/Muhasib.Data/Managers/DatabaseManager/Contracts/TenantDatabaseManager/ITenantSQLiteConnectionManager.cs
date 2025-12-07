using Muhasib.Data.Managers.DatabaseManager.Models;
using Muhasib.Domain.Enum;

namespace Muhasib.Data.Managers.DatabaseManager.Contracts.TenantDatabaseManager
{
    public interface ITenantSQLiteConnectionManager
    {
        // Saf connection management
        Task<string> GetConnectionStringAsync(string databaseName);
        Task<ConnectionTestResult> TestConnectionDetailedAsync(string databaseName);
        Task<(bool IsValid, string Message)> ValidateTenantAsync(string databaseName);
        Task<string> GetConnectionStringInfoAsync(string databaseName);

    }
}
