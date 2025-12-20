namespace Muhasib.Data.DataContext.Factories
{
    public interface IAppDbContextFactory
    {
        AppDbContext CreateContext(string databaseName);
        Task<bool> TestConnectionAsync(string databaseName, CancellationToken cancellationToken = default);

        // Opsiyonel - sadece ihtiyaç varsa ekleyin
        bool TenantDatabaseFileExists(string databaseName);
        bool IsTenantDatabaseSizeValid(string databaseName);
        string GetTenantDatabaseFilePath(string databaseName);
        long GetDatabaseSize(string databaseName);
    }
}
