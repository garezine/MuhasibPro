using Muhasebe.Business.Models.UpdateModels;
using Muhasebe.Domain.Entities.SistemDb;

namespace Muhasebe.Business.Services.Abstract.Update
{
    public interface IUpdateService
    {
        Task<UpdateInfo> CheckForUpdatesAsync();
        Task<string> DownloadUpdateFile(string downloadUrl);
        Task<UpdateSettings> CheckForUpdatesOnSettings();
        Task UpdateLastCheckDateAsync();
        Task<UpdateSettings> GetUpdateSettings();
        Task SaveSettingsAsync(UpdateSettings updateSettings);
        Task<string> DownloadUpdateFile(string downloadUrl,
            string expectedHash,
            IProgress<(long downloaded, long total, double speed)> progress = null);
        Task<DeltaUpdateInfo> CheckForDeltaUpdateAsync();

        Task<PendingUpdateInfo> GetPendingUpdateAsync();
        Task SavePendingUpdateAsync(PendingUpdateInfo pendingInfo);
        Task ClearPendingUpdateAsync();
        Task<bool> VerifyUpdateFileAsync(string filePath, string expectedHash);
    }


}
