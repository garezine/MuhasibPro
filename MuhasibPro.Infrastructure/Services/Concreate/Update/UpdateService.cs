using System.Diagnostics;
using MuhasibPro.Infrastructure.Models.Update;
using MuhasibPro.Infrastructure.Services.Abstract.Common;
using Velopack;
using Velopack.Sources;

namespace MuhasibPro.Infrastructure.Services.Concreate.Update;

public class UpdateService : IUpdateService
{
    private UpdateManager? _updateManager;
    private UpdateSettings? _cachedSettings;
    private UpdateInfo _updateInfo;

    private const string REPO_URL = "https://github.com/garezine/MuhasibPro";
    private readonly string? _githubToken = null;

    public bool IsUpdatePendingRestart => _updateManager?.UpdatePendingRestart != null;

    public UpdateService()
    {
        EnsureManager(_lastPrereleaseFlag);
    }
    private void EnsureManager(bool includePrereleases)
    {
        // includePrereleases değiştiyse manager'ı yeniden kur
        if (_updateManager == null || (_updateManager is not null && _lastPrereleaseFlag != includePrereleases))
        {
            var source = new GithubSource(
                repoUrl: REPO_URL,
                accessToken: _githubToken,
                prerelease: includePrereleases
            );
            _updateManager = new UpdateManager(source);
            _lastPrereleaseFlag = includePrereleases;
        }
    }

    private bool _lastPrereleaseFlag = false;
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
        EnsureManager(includePrereleases);

        try
        {
            if (_updateManager == null) throw new InvalidOperationException("UpdateManager oluşturulamadı.");
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

    public async Task<bool> DownloadUpdatesAsync(IProgress<int>? progress = null, CancellationToken ct = default)
    
    {
        if (_updateManager == null) throw new InvalidOperationException("UpdateManager mevcut değil.");
        if (_updateInfo?.TargetFullRelease == null) throw new InvalidOperationException("Önce CheckForUpdatesAsync çağrılmalı ve güncelleme bulunmalı.");
        try
        {
            Debug.WriteLine("Güncelleme indiriliyor...");
            await _updateManager.DownloadUpdatesAsync(_updateInfo, p => progress?.Report(p), ct);
            Debug.WriteLine("İndirme tamamlandı.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"İndirme hatası: {ex}");
            throw;
        }
    }

    public void ApplyUpdatesAndRestart(params string[] restartArgs)
    {
        if (_updateManager == null) throw new InvalidOperationException("UpdateManager mevcut değil.");

        // Önce pending varsa onu uygula, yoksa CheckForUpdates'tan gelen target'ı kullan
        var asset = _updateManager.UpdatePendingRestart ?? _updateInfo?.TargetFullRelease;
        if (asset == null) throw new InvalidOperationException("Uygulanacak güncelleme bulunamadı.");

        try
        {
            Debug.WriteLine("Güncelleme uygulanıyor ve uygulama yeniden başlatılıyor...");
            _updateManager.ApplyUpdatesAndRestart(asset, restartArgs);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Uygulama sırasında hata: {ex}");
            throw;
        }
    }
}
