using Microsoft.EntityFrameworkCore;
using Muhasebe.Data.Abstract.Common;
using Muhasebe.Data.DataContext;
using Muhasebe.Domain.Entities.SistemDb;

namespace Muhasebe.Data.Database.Concreate
{
    public class BackupScheduleRepository : IBackupScheduleRepository
    {
        private readonly AppSistemDbContext _context;

        public BackupScheduleRepository(AppSistemDbContext context)
        {
            _context = context;
        }

        // Aktif yedekleme planlarını getir
        public async Task<List<DbYedekAl>> GetActiveSchedulesAsync()
        {
            return await _context.DbYedekAl
                .Where(s => s.AktifMi)
                .ToListAsync().ConfigureAwait(false);
        }

        // Sonraki yedek tarihini güncelle
        public async Task UpdateNextBackupDateAsync(long scheduleId, DateTime nextDate)
        {
            var schedule = await _context.DbYedekAl
                .FirstOrDefaultAsync(s => s.Id == scheduleId).ConfigureAwait(false);

            if (schedule != null)
            {
                schedule.SonrakiYedekTarih = nextDate;
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        // İsteğe bağlı: Yeni yedek planı ekleme
        public async Task AddScheduleAsync(DbYedekAl schedule)
        {
            await _context.DbYedekAl.AddAsync(schedule).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        // İsteğe bağlı: Yedek planını pasif hale getirme
        public async Task DeactivateScheduleAsync(int scheduleId)
        {
            var schedule = await _context.DbYedekAl.FindAsync(scheduleId).ConfigureAwait(false);
            if (schedule != null)
            {
                schedule.AktifMi = false;
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
