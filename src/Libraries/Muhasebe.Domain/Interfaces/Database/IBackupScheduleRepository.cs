using Muhasebe.Domain.Entities.Sistem;

namespace Muhasebe.Domain.Interfaces.Database
{
    public interface IBackupScheduleRepository
    {
        Task<List<DbYedekZaman>> GetActiveSchedulesAsync();
        Task UpdateNextBackupDateAsync(long scheduleId, DateTime nextDate);
    }
}
