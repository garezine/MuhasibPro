namespace Muhasebe.Data.Database.SistemDatabase
{
    public interface ISistemDatabaseManager
    {
        Task<bool> InitializeSistemDatabaseAsync();
        Task<bool> ValidateSistemDatabaseAsync();
        Task<string> GetCurrentSchemaVersionAsync();
        Task<List<string>> GetPendingMigrationsAsync();
        Task<List<string>> GetAppliedMigrationsAsync();
        Task<MigrationStatus> CheckMigrationStatusAsync();
        Task<bool> BackupDatabaseAsync(string backupPath);
        Task<bool> RestoreDatabaseAsync(string backupPath);
    }
}
