using Muhasebe.Data.Database.Helpers;

namespace Muhasebe.Data.Database.Interfaces.Operations
{
    public interface IDatabaseDeletionOperations
    {
        Task<DeletionResult> DeleteDatabaseAsync(string connectionString, string dbName, string dbDirectory, string dbPath);
    }
}
