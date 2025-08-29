using System.Diagnostics;
using MuhasibPro.Core.Models.Update;
using MuhasibPro.Core.Services.Abstract.Common;
using Velopack;

namespace MuhasibPro.Core.Services.Concreate.Update;

public class UpdateService : IUpdateService
{
    private readonly UpdateManager? _updateManager;    
    private UpdateSettings? _cachedSettings;
    private UpdateInfo _updateInfo;

    // GitHub release feed URL
    private const string GITHUB_REPO_URL = "https://github.com/garezine/MuhasibPro";

    public bool IsUpdatePendingRestart => _updateManager?.IsUpdatePendingRestart ?? false;

    public UpdateService()
    {        
        _updateManager = new UpdateManager(GITHUB_REPO_URL);
    }

    public async Task<UpdateSettings> GetSettingsAsync()
    {
        if (_cachedSettings == null)
        {
            _cachedSettings = await UpdateSettings.LoadAsync();
        }
        return _cachedSettings;
    }

    public async Task SaveSettingsAsync(UpdateSettings settings)
    {
        _cachedSettings = settings;
        await settings.SaveAsync();
        Debug.Write("Güncelleme ayarları kaydedildi");
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync(bool includePrereleases = false)
    {
        if (_updateManager == null)
        {
            Debug.Write("Güncelleme işlemi başlatılamadı");
            return null;
        }

        try
        {
           Debug.Write("Güncellemeler kontrol ediliyor...");

            _updateInfo = await _updateManager.CheckForUpdatesAsync();

            // Settings'de son kontrol tarihini güncelle
            var settings = await GetSettingsAsync();
            settings.LastCheckDate = DateTime.Now;
            await SaveSettingsAsync(settings);

            if (_updateInfo == null)
            {
                Debug.Write("Güncelleme bulunamadı");
                return null;
            }
            Debug.Write($"Güncelleme bulundu: {_updateInfo.TargetFullRelease}");
            return _updateInfo;
        }
        catch (Exception ex)
        {
            Debug.Write(ex, "Güncelleme kontrolü sırasında hata");
            throw;
        }
    }

    public async Task<bool> DownloadUpdatesAsync(IProgress<int>? progress = null)
    {
        if (_updateManager == null)
        {
            throw new InvalidOperationException("UpdateManager mevcut değil");
        }
        Action<int> progressAction = (percent) =>
        {
            Debug.Write($"İndirme ilerlemesi: {percent}%");
            progress?.Report(percent);
        };

        try
        {
            Debug.Write("Güncelleme indiriliyor...");

            await _updateManager.DownloadUpdatesAsync(_updateInfo,progressAction);

            Debug.Write("Güncelleme başarıyla indirildi");
            return true;
        }
        catch (Exception ex)
        {
            Debug.Write(ex, "Güncelleme indirme sırasında hata");
            throw;
        }
    }

    public void ApplyUpdatesAndRestart()
    {
        if (_updateManager == null)
        {
            throw new InvalidOperationException("UpdateManager mevcut değil");
        }

        try
        {
            Debug.Write("Güncelleme uygulanıyor ve yeniden başlatılıyor...");
            _updateManager.ApplyUpdatesAndRestart(_updateInfo);
        }
        catch (Exception ex)
        {
            Debug.Write(ex, "Güncelleme uygulama sırasında hata");
            throw;
        }
    }
}
