using Muhasib.Business.Services.Contracts.DatabaseServices.SistemDatabase;
using Muhasib.Data.Managers.UpdataManager;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using System.Diagnostics;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;

namespace MuhasibPro.Services.Infrastructure.CommonServices;
public class UpdateService : IUpdateService
{
    private UpdateManager? _updateManager;
    private UpdateSettingsModel? _cachedSettings;
    private UpdateInfo _updateInfo;

    //private readonly ISistemDatabaseUpdateService _databaseUpdateService;
    private readonly ILocalUpdateManager _localUpdateManager;

    private const string REPO_URL = "https://github.com/garezine/MuhasibPro";
    private readonly string? _githubToken = null;

    public bool IsUpdatePendingRestart => _updateManager?.UpdatePendingRestart != null;

    public UpdateService(ILocalUpdateManager localUpdateManager)
    {
        //_databaseUpdateService = databaseUpdateService;
        _localUpdateManager = localUpdateManager;
        EnsureManager(true);
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
            _updateManager = new UpdateManager(source, locator: VelopackLocator.Current);
            _lastPrereleaseFlag = includePrereleases;
        }
    }

    private bool _lastPrereleaseFlag = false;

    public async Task<UpdateSettingsModel> GetSettingsAsync()
    {
        if (_cachedSettings == null)
        {
            System.Diagnostics.Debug.WriteLine("Loading settings from LocalUpdateManager...");
            _cachedSettings = await _localUpdateManager.LoadAsync();

            System.Diagnostics.Debug.WriteLine($"Loaded - AutoCheck: {_cachedSettings?.AutoCheckOnStartup}, Notifications: {_cachedSettings?.ShowNotifications}");

            if (_cachedSettings == null)
            {
                System.Diagnostics.Debug.WriteLine("Settings were null, creating default...");
                _cachedSettings = new UpdateSettingsModel();
                await SaveSettingsAsync(_cachedSettings);
            }
        }
        return _cachedSettings;
    }

    public async Task SaveSettingsAsync(UpdateSettingsModel settings)
    {
        _cachedSettings = settings;
        await _localUpdateManager.SaveAsync(settings);
        Debug.Write("Güncelleme ayarları kaydedildi");
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync(bool includePrereleases = false)
    {
        EnsureManager(includePrereleases);

        try
        {
            if (_updateManager == null)
                throw new InvalidOperationException("UpdateManager oluşturulamadı.");
            Debug.Write("Güncellemeler kontrol ediliyor...");

            _updateInfo = await _updateManager.CheckForUpdatesAsync();

            // Settings'de son kontrol tarihini güncelle
            var settings = await GetSettingsAsync();
            settings.LastCheckTime = DateTime.Now;

            settings.IncludeBetaVersions = includePrereleases;

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
        if (_updateManager == null)
            throw new InvalidOperationException("UpdateManager mevcut değil.");
        if (_updateInfo?.TargetFullRelease == null)
            throw new InvalidOperationException("Önce CheckForUpdatesAsync çağrılmalı ve güncelleme bulunmalı.");
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
        if (_updateManager == null)
            throw new InvalidOperationException("UpdateManager mevcut değil.");

        // Önce pending varsa onu uygula, yoksa CheckForUpdates'tan gelen target'ı kullan
        var asset = _updateManager.UpdatePendingRestart ?? _updateInfo?.TargetFullRelease;
        if (asset == null)
            throw new InvalidOperationException("Uygulanacak güncelleme bulunamadı.");

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

    #region Veritabanı Güncelleme İşlemleri
    public Task<bool> PrepareForUpdateAsync()
    {
        try
        {
            Debug.WriteLine("Güncelleme öncesi database hazırlığı başlatılıyor...");

            // Mevcut veritabanlarının durumunu kontrol et
            //var systemStatus = await _databaseUpdateService.GetOverallSystemStatusAsync();
            Debug.WriteLine($"Database durumunda hata");

            // Kritik: Güncelleme öncesi backup
            // Bu metodu DatabaseUpdateService'e ekleyeceğiz

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Database hazırlık hatası: {ex}");
            return Task.FromResult(false);
        }
    }

    public Task<bool> PostUpdateDatabaseSyncAsync()
    {
        try
        {
            Debug.WriteLine("Güncelleme sonrası database senkronizasyonu başlatılıyor...");

            // Tüm veritabanlarını yeni versiyona göre güncelle
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Post-update database sync hatası: {ex}");
            return Task.FromResult(false);
        }
    }

    public async void ApplyUpdatesAndRestartWithDatabaseSync(params string[] restartArgs)
    {
        // Önce database sync
        var dbSyncSuccess = await PostUpdateDatabaseSyncAsync();
        if (!dbSyncSuccess)
        {
            Debug.WriteLine("UYARI: Database senkronizasyonu başarısız!");
            // Yine de güncellemeye devam et ama log'la
        }

        // Normal güncelleme işlemi
        ApplyUpdatesAndRestart(restartArgs);
    }
    #endregion
}

