namespace Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager
{
    public interface ITenantMigrationManager
    {
        Task<bool> PrepareDatabaseAsync(long maliDonemId);
        Task<List<string>> GetPendingMigrationsAsync(long maliDonemId);
        Task<bool> RunMigrationsAsync(long maliDonemId);
        Task<bool> DatabaseExistsAsync(string databaseName);
        Task<bool> TableExistsAsync(long maliDonemId, string tableName);
    }
}
