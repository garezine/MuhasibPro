namespace Muhasib.Data.Managers.DatabaseManager.Contracts.TenantDatabaseManager
{
    public interface ITenantSQLiteMigrationManager
    {           
        Task<bool> RunMigrationsAsync(string databaseName,CancellationToken cancellationToken=default);
        Task<bool> InitializingDatabaseAsync(string databaseName, CancellationToken cancellationToken = default);
        Task<IList<string>> GetPendingMigrationsAsync(string databaseName);
    }
}
