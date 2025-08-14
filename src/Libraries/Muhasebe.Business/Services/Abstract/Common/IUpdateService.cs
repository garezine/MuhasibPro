using Muhasebe.Business.Helpers;
using Muhasebe.Domain.Entities.Sistem;

namespace Muhasebe.Business.Services.Abstract.Common
{
    public interface IUpdateService
    {
        Task<UpdateInfo> CheckForUpdatesAsync();
        Task<string> DownloadUpdateFile(string downloadUrl);
        Task<UpdateSettings> CheckForUpdatesOnSettings();
        Task UpdateLastCheckDateAsync();
        Task<UpdateSettings> GetUpdateSettings();
        Task SaveSettingsAsync(UpdateSettings updateSettings);
        Task<string> DownloadUpdateFile(string downloadUrl, IProgress<(long downloaded, long total, double speed)> progress = null);
        Task<DeltaUpdateInfo> CheckForDeltaUpdateAsync();

    }


}
