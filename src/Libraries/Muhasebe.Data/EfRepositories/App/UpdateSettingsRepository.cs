using Microsoft.EntityFrameworkCore;
using Muhasebe.Data.DataContext;
using Muhasebe.Domain.Entities.SistemDb;
using Muhasebe.Domain.Interfaces.App;

namespace Muhasebe.Data.EfRepositories.App
{
    public class UpdateSettingsRepository : IUpdateSettingsRepository
    {
        private readonly AppSistemDbContext _dbContext;

        public UpdateSettingsRepository(AppSistemDbContext dbContext) { _dbContext = dbContext; }

        
            

        public async Task<UpdateSettings> GetSettingsAsync()
        {
            var settings = await _dbContext.UpdateSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                // İlk kez çalışıyorsa default ayarları oluştur
                settings = new UpdateSettings();                  
                await _dbContext.AddAsync(settings);
                await _dbContext.SaveChangesAsync();
            }

            return settings;
        }

        public async Task SaveSettingsAsync(UpdateSettings settings)
        {          
            _dbContext.Update(settings);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> ShouldCheckForUpdatesAsync()
        {
            var settings = await GetSettingsAsync();

            if (!settings.AutoCheckOnStartup)
                return false;

            if (settings.LastCheckDate == null)
                return true;

            var timeSinceLastCheck = DateTime.Now - settings.LastCheckDate.Value;
            return timeSinceLastCheck.TotalHours >= settings.CheckIntervalHours;
        }

        public async Task UpdateLastCheckDateAsync()
        {
            var settings = await GetSettingsAsync();
            settings.LastCheckDate = DateTime.Now;
            await SaveSettingsAsync(settings);
        }
    }
}

