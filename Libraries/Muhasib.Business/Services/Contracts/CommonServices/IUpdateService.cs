// MuhasibPro.Contracts/Services/IUpdateService.cs
using Muhasib.Data.Managers.UpdataManager;
using Velopack;

namespace Muhasib.Business.Services.Contracts.CommonServices
{
    public interface IUpdateService
    {
        // Events
        Task<UpdateSettingsModel> GetSettingsAsync();
        Task SaveSettingsAsync(UpdateSettingsModel settings);
        Task<UpdateInfo?> CheckForUpdatesAsync(bool includePrereleases = false);
        Task<bool> DownloadUpdatesAsync(IProgress<int>? progress = null, CancellationToken ct = default);
        void ApplyUpdatesAndRestart(params string[] restartArgs);
        void ApplyUpdatesAndRestartWithDatabaseSync(params string[] restartArgs);
        bool IsUpdatePendingRestart { get; }

        // Veritabanı güncelleme işlemleri

        Task<bool> PrepareForUpdateAsync();
        Task<bool> PostUpdateDatabaseSyncAsync();
    }
}