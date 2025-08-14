using Microsoft.EntityFrameworkCore;
using Muhasebe.Data.DataContext;
using Muhasebe.Domain.Entities.Sistem;
using Muhasebe.Domain.Interfaces.Database;

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
        public async Task<List<DbYedekZaman>> GetActiveSchedulesAsync()
        {
            return await _context.DbYedekZamanlama
                .Where(s => s.AktifMi)
                .ToListAsync().ConfigureAwait(false);
        }

        // Sonraki yedek tarihini güncelle
        public async Task UpdateNextBackupDateAsync(long scheduleId, DateTime nextDate)
        {
            var schedule = await _context.DbYedekZamanlama
                .FirstOrDefaultAsync(s => s.Id == scheduleId).ConfigureAwait(false);

            if (schedule != null)
            {
                schedule.SonrakiYedekTarih = nextDate;
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        // İsteğe bağlı: Yeni yedek planı ekleme
        public async Task AddScheduleAsync(DbYedekZaman schedule)
        {
            await _context.DbYedekZamanlama.AddAsync(schedule).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        // İsteğe bağlı: Yedek planını pasif hale getirme
        public async Task DeactivateScheduleAsync(int scheduleId)
        {
            var schedule = await _context.DbYedekZamanlama.FindAsync(scheduleId).ConfigureAwait(false);
            if (schedule != null)
            {
                schedule.AktifMi = false;
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
