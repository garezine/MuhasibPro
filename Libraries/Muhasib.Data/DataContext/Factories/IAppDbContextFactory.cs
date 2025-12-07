namespace Muhasib.Data.DataContext.Factories
{
    public interface IAppDbContextFactory
    {
        AppDbContext CreateContext(string databaseName);
        Task<bool> TestConnectionAsync(string databaseName, CancellationToken cancellationToken = default);

        // Opsiyonel - sadece ihtiyaç varsa ekleyin
        bool TenantDatabaseFileExists(string databaseName);
        bool IsDatabaseSizeValid(string databaseName);
        string GetTenantDatabaseFilePath(string databaseName);
        long GetDatabaseSize(string databaseName);       
    }
}
