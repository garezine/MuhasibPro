namespace Muhasebe.Data.Database.Interfaces.Services
{
    public interface IDatabaseMaintenanceService
    {
        Task CheckDatabaseIntegrityAsync(long fId, long dId);
        Task ReindexDatabaseAsync(long fId, long dId);
    }
}
