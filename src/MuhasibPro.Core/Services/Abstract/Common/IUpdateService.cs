using MuhasibPro.Core.Models.Update;
using Velopack;


namespace MuhasibPro.Core.Services.Abstract.Common;
public interface IUpdateService
{
    Task<UpdateSettings> GetSettingsAsync();
    Task SaveSettingsAsync(UpdateSettings settings);
    Task<UpdateInfo?> CheckForUpdatesAsync(bool includePrereleases = false);
    Task<bool> DownloadUpdatesAsync(IProgress<int>? progress = null, CancellationToken ct = default);
    void ApplyUpdatesAndRestart(params string[] restartArgs);
    bool IsUpdatePendingRestart { get; }
}
