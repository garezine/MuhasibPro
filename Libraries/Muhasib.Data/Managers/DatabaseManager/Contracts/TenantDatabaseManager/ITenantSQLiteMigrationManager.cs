namespace Muhasib.Data.Managers.DatabaseManager.Contracts.TenantDatabaseManager
{
    public interface ITenantSQLiteMigrationManager
    {
        Task<bool> InitializeDatabaseAsync(string databaseName);
        Task<List<string>> GetPendingMigrationsAsync(string databaseName);
        Task<bool> RunMigrationsAsync(string databaseName);            
    }
}
