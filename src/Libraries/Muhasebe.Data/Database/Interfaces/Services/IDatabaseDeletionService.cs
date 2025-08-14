using Muhasebe.Data.Database.Helpers;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Data.Database.Interfaces.Services
{
    public interface IDatabaseDeletionService
    {
        Task<DeletionResult> DeletePhysicalDatabaseAsync(string dbName, string dbDirectory, string dbPathOrIdentifier, DatabaseType dbType);
    }
}
