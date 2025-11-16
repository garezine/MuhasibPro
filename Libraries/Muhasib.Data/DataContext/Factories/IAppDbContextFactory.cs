namespace Muhasib.Data.DataContext.Factories
{
    public interface IAppDbContextFactory
    {
        AppDbContext CreateForTenant(long maliDonemId);
        Task<AppDbContext> CreateForTenantAsync(long maliDonemId);
        Task<AppDbContext> CreateForDatabaseAsync(string databaseName);
        AppDbContext CreateForDatabase(string databaseName);
    }
}
