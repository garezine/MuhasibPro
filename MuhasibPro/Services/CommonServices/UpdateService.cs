using MuhasibPro.Contracts.CommonServices;
using MuhasibPro.Contracts.SistemServices.DatabaseServices;
using MuhasibPro.Helpers;
using System.Diagnostics;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;

namespace MuhasibPro.Services.CommonServices;

public class UpdateService : IUpdateService
{
    private UpdateManager? _updateManager;
    private UpdateSettingsModel? _cachedSettings;
    private UpdateInfo _updateInfo;

    private readonly IDatabaseUpdateService _databaseUpdateService;

    private const string REPO_URL = "https://github.com/garezine/MuhasibPro";
    private readonly string? _githubToken = null;

    public bool IsUpdatePendingRestart => _updateManager?.UpdatePendingRestart != null;

    public UpdateService(IDatabaseUpdateService databaseUpdateService)
    { 
        _databaseUpdateService = databaseUpdateService;
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
            _updateManager = new UpdateManager(source,locator:VelopackLocator.Current);
            _lastPrereleaseFlag = includePrereleases;
        }
    }

    private bool _lastPrereleaseFlag = false;

    public async Task<UpdateSettingsModel> GetSettingsAsync()
    {
        if(_cachedSettings == null)
        {
            _cachedSettings = await UpdateHelper.LoadAsync();
        }
        return _cachedSettings;
    }

    public async Task SaveSettingsAsync(UpdateSettingsModel settings)
    {
        _cachedSettings = settings;
        await UpdateHelper.SaveAsync(settings);
        Debug.Write("Güncelleme ayarları kaydedildi");
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync(bool includePrereleases = false)
    {
        EnsureManager(includePrereleases);

        try
        {
            if(_updateManager == null)
                throw new InvalidOperationException("UpdateManager oluşturulamadı.");
            Debug.Write("Güncellemeler kontrol ediliyor...");

            _updateInfo = await _updateManager.CheckForUpdatesAsync();

            // Settings'de son kontrol tarihini güncelle
            var settings = await GetSettingsAsync();
            settings.LastCheckDate = DateTime.Now;
            await SaveSettingsAsync(settings);

            if(_updateInfo == null)
            {
                Debug.Write("Güncelleme bulunamadı");
                return null;
            }
            Debug.Write($"Güncelleme bulundu: {_updateInfo.TargetFullRelease}");
            return _updateInfo;
        } catch(Exception ex)
        {
            Debug.Write(ex, "Güncelleme kontrolü sırasında hata");
            throw;
        }
    }

    public async Task<bool> DownloadUpdatesAsync(IProgress<int>? progress = null, CancellationToken ct = default)

    {
        if(_updateManager == null)
            throw new InvalidOperationException("UpdateManager mevcut değil.");
        if(_updateInfo?.TargetFullRelease == null)
            throw new InvalidOperationException("Önce CheckForUpdatesAsync çağrılmalı ve güncelleme bulunmalı.");
        try
        {
            Debug.WriteLine("Güncelleme indiriliyor...");
            await _updateManager.DownloadUpdatesAsync(_updateInfo, p => progress?.Report(p), ct);
            Debug.WriteLine("İndirme tamamlandı.");
            return true;
        } catch(Exception ex)
        {
            Debug.WriteLine($"İndirme hatası: {ex}");
            throw;
        }
    }

    public void ApplyUpdatesAndRestart(params string[] restartArgs)
    {
        if(_updateManager == null)
            throw new InvalidOperationException("UpdateManager mevcut değil.");

        // Önce pending varsa onu uygula, yoksa CheckForUpdates'tan gelen target'ı kullan
        var asset = _updateManager.UpdatePendingRestart ?? _updateInfo?.TargetFullRelease;
        if(asset == null)
            throw new InvalidOperationException("Uygulanacak güncelleme bulunamadı.");

        try
        {
            Debug.WriteLine("Güncelleme uygulanıyor ve uygulama yeniden başlatılıyor...");
            _updateManager.ApplyUpdatesAndRestart(asset, restartArgs);
        } catch(Exception ex)
        {
            Debug.WriteLine($"Uygulama sırasında hata: {ex}");
            throw;
        }
    }

    #region Veritabanı Güncelleme İşlemleri
    public async Task<bool> PrepareForUpdateAsync()
    {
        try
        {
            Debug.WriteLine("Güncelleme öncesi database hazırlığı başlatılıyor...");

            // Mevcut veritabanlarının durumunu kontrol et
            var systemStatus = await _databaseUpdateService.GetOverallSystemStatusAsync();
            Debug.WriteLine($"Database durumu: {systemStatus}");

            // Kritik: Güncelleme öncesi backup
            // Bu metodu DatabaseUpdateService'e ekleyeceğiz

            return true;
        } catch(Exception ex)
        {
            Debug.WriteLine($"Database hazırlık hatası: {ex}");
            return false;
        }
    }

    public async Task<bool> PostUpdateDatabaseSyncAsync()
    {
        try
        {
            Debug.WriteLine("Güncelleme sonrası database senkronizasyonu başlatılıyor...");

            // Tüm veritabanlarını yeni versiyona göre güncelle
            return await _databaseUpdateService.UpdateAllDatabasesAsync();
        } catch(Exception ex)
        {
            Debug.WriteLine($"Post-update database sync hatası: {ex}");
            return false;
        }
    }

    public async void ApplyUpdatesAndRestartWithDatabaseSync(params string[] restartArgs)
    {
        // Önce database sync
        var dbSyncSuccess = await PostUpdateDatabaseSyncAsync();
        if(!dbSyncSuccess)
        {
            Debug.WriteLine("UYARI: Database senkronizasyonu başarısız!");
            // Yine de güncellemeye devam et ama log'la
        }

        // Normal güncelleme işlemi
        ApplyUpdatesAndRestart(restartArgs);
    }
    #endregion
}
