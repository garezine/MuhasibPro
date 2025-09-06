using Muhasebe.Domain.Entities.SistemDb;

namespace Muhasebe.Data.Abstract.Common
{
    public interface IBackupScheduleRepository
    {
        Task<List<DbYedekAl>> GetActiveSchedulesAsync();
        Task UpdateNextBackupDateAsync(long scheduleId, DateTime nextDate);
    }
}
