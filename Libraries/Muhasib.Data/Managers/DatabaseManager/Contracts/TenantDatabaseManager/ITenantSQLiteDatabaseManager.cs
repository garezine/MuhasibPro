using Muhasib.Data.Managers.DatabaseManager.Models;

namespace Muhasib.Data.Managers.DatabaseManager.Contracts.TenantSqliteManager
{
    public interface ITenantSQLiteDatabaseManager
    {
        /// <summary>
        /// Yeni database oluşturur (migration'ları çalıştırır)
        /// </summary>
        Task<bool> CreateOrUpdateDatabaseAsync(string databaseName, CancellationToken cancellationToken = default);
        /// <summary>
        /// Database'i siler
        /// </summary>
        Task<bool> DeleteDatabaseAsync(string databaseName, CancellationToken cancellationToken = default);
        Task<DatabaseHealthInfo> GetHealthStatusAsync(string databaseName, CancellationToken cancellationToken = default);        
        Task<(bool IsValid, string Message)> ValidateTenantDatabaseAsync(string databaseName,CancellationToken cancellationToken = default);


    }
}
