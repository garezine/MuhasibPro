namespace Muhasib.Data.Managers.DatabaseManager.Contracts.SistemDatabase
{
    public interface ISistemMigrationManager
    {
        Task<bool> InitializeDatabaseAsync(CancellationToken cancellationToken = default);
        Task<List<string>> GetPendingMigrationsAsync();
        Task<bool> RunMigrationsAsync(CancellationToken cancellationToken = default);
    }
}
