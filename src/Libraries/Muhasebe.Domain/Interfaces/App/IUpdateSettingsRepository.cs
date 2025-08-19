using Muhasebe.Domain.Entities.SistemDb;

namespace Muhasebe.Domain.Interfaces.App
{
    public interface IUpdateSettingsRepository
    {
        Task<UpdateSettings> GetSettingsAsync();
        Task SaveSettingsAsync(UpdateSettings settings);
        Task<bool> ShouldCheckForUpdatesAsync();
        Task UpdateLastCheckDateAsync();
    }
}
