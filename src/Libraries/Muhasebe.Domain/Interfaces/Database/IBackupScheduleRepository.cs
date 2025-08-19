using Muhasebe.Domain.Entities.SistemDb;

namespace Muhasebe.Domain.Interfaces.Database
{
    public interface IBackupScheduleRepository
    {
        Task<List<DbYedekAl>> GetActiveSchedulesAsync();
        Task UpdateNextBackupDateAsync(long scheduleId, DateTime nextDate);
    }
}
