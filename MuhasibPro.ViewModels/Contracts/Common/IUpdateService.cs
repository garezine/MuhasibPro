using MuhasibPro.ViewModels.Helpers;
using Velopack;

namespace MuhasibPro.ViewModels.Contracts.Common;
public interface IUpdateService
{
    Task<UpdateSettingsModel> GetSettingsAsync();
    Task SaveSettingsAsync(UpdateSettingsModel settings);
    Task<UpdateInfo?> CheckForUpdatesAsync(bool includePrereleases = false);
    Task<bool> DownloadUpdatesAsync(IProgress<int>? progress = null, CancellationToken ct = default);
    void ApplyUpdatesAndRestart(params string[] restartArgs);
    bool IsUpdatePendingRestart { get; }
}
