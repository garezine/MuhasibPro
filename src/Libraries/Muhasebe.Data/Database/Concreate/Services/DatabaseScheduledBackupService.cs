using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.Abstract.Common;
using Muhasebe.Data.Database.Interfaces.Services;
using Muhasebe.Domain.Entities.SistemDb;

namespace Muhasebe.Data.Database.Concreate.Services
{
    // EnvanterPro.Business/Services/DatabaseManagement/Concrete/ScheduledBackupService.cs
    public class DatabaseScheduledBackupService : BackgroundService
    {
        private readonly IBackupScheduleRepository _scheduleRepo;
        private readonly IDatabaseBackupService _backupService;
        private readonly ILogger<DatabaseScheduledBackupService> _logger;

        public DatabaseScheduledBackupService(
            IBackupScheduleRepository scheduleRepo,
            IDatabaseBackupService backupService,
            ILogger<DatabaseScheduledBackupService> logger)
        {
            _scheduleRepo = scheduleRepo;
            _backupService = backupService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var schedules = await _scheduleRepo.GetActiveSchedulesAsync().ConfigureAwait(false);
                foreach (var schedule in schedules)
                {
                    if (schedule.SonrakiYedekTarih <= DateTime.Now)
                    {
                        await TriggerBackup(schedule).ConfigureAwait(false);
                        await UpdateNextSchedule(schedule).ConfigureAwait(false);
                    }
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken).ConfigureAwait(false); // Her 1 dakikada bir kontrol
            }
        }

        private async Task TriggerBackup(DbYedekAl schedule)
        {
            try
            {
                await _backupService.BackupDatabaseAsync(
                    fId: schedule.FirmaId,
                    dId: schedule.DonemId, // Aktif dönemi kullanın veya parametre ekleyin
                    backupDirectory: "C:/AutoBackups"
                ).ConfigureAwait(false);
                _logger.LogInformation($"Yedek alındı: FirmaId={schedule.FirmaId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Yedek hatası: FirmaId={schedule.FirmaId}");
            }
        }

        private async Task UpdateNextSchedule(DbYedekAl schedule)
        {
            schedule.SonrakiYedekTarih = CalculateNextBackupDate(schedule);
            await _scheduleRepo.UpdateNextBackupDateAsync(schedule.Id, schedule.SonrakiYedekTarih).ConfigureAwait(false);
        }

        private DateTime CalculateNextBackupDate(DbYedekAl schedule)
        {
            var nextDate = schedule.YedeklemeAraligi switch
            {
                "Daily" => DateTime.Now.AddDays(1),
                "Weekly" => DateTime.Now.AddDays(7),
                "Monthly" => DateTime.Now.AddMonths(1),
                _ => throw new ArgumentException("Geçersiz sıklık")
            };
            return nextDate.Date + schedule.YedeklemeSaati;
        }
    }
}