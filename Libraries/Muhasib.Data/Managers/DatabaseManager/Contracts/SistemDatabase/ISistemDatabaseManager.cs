using Muhasib.Data.Managers.DatabaseManager.Models;

namespace Muhasib.Data.Managers.DatabaseManager.Contracts.SistemDatabase
{
    public interface ISistemDatabaseManager
    {
        Task<bool> InitializeDatabaseAsync(CancellationToken cancellationToken = default);
        Task<DatabaseHealthInfo> GetHealthStatusAsync(CancellationToken cancellationToken = default);       
        Task<(bool IsValid, string Message)> ValidateSistemDatabaseAsync(CancellationToken cancellationToken = default);



    }
}
