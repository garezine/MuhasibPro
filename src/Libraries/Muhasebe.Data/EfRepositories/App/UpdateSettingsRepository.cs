using Muhasebe.Data.DataContext;
using Muhasebe.Domain.Entities.Sistem;
using Muhasebe.Domain.Interfaces.App;
using Muhasebe.Domain.Interfaces.Database;
using Muhasebe.Domain.Utilities.IDGenerator;

namespace Muhasebe.Data.EfRepositories.App
{
    public class UpdateSettingsRepository : IUpdateSettingsRepository
    {
        private readonly IUnitOfWork<AppSistemDbContext> _unitOfWork;

        public UpdateSettingsRepository(IUnitOfWork<AppSistemDbContext> unitOfWork) { _unitOfWork = unitOfWork; }

        private IGenericRepository<UpdateSettings> GenericRepository => _unitOfWork.GetRepository<IGenericRepository<UpdateSettings>>(
            );

        public async Task<UpdateSettings> GetSettingsAsync()
        {
            var settings = await GenericRepository.FirstOrDefaultAsync(predicate: s => true);

            if (settings == null)
            {
                // İlk kez çalışıyorsa default ayarları oluştur
                settings = new UpdateSettings
                {
                    Id = UIDModuleGenerator.GenerateModuleId(UIDModuleType.Sistem),
                    KayitTarihi = DateTime.Now,
                    AktifMi = true,
                    KaydedenId = 1 // İlk kayıt için 0 veya uygun bir ID kullanabilirsiniz
                };
                await GenericRepository.AddAsync(settings);
                await _unitOfWork.CommitAsync();
            }

            return settings;
        }

        public async Task SaveSettingsAsync(UpdateSettings settings)
        {
            settings.GuncellemeTarihi = DateTime.Now;
            await GenericRepository.UpdateAsync(settings);
            await _unitOfWork.CommitAsync();
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

