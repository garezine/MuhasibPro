using Microsoft.UI.Xaml;
using Muhasebe.Business.Models.UpdateModels;
using Muhasebe.Business.Services.Abstract.Update;
using Muhasebe.Domain.Entities.SistemDb;
using MuhasibPro.Core.Models.Update;
using MuhasibPro.Core.Services.Abstract.Common;
using System.Diagnostics;
using Windows.Management.Deployment;

namespace MuhasibPro.Core.Services.Concreate.Update
{
    public class UpdateManager
    {
        private readonly IDeltaAnalyzer _deltaAnalyzer;
        private readonly IDeltaDownloader _deltaDownloader;
        private readonly IUpdateService _updateService;
        private readonly IMessageService _messageService;

        public bool HasPendingUpdate { get; private set; }

        public UpdateInfo PendingUpdateInfo { get; private set; }

        public string PendingUpdateLocalPath { get; private set; }

        // Mevcut güncelleme bilgilerini tutmak için ek property
        private UpdateInfo _currentAvailableUpdate;

        public UpdateManager(
            IUpdateService updateService,
            IDeltaAnalyzer deltaAnalyzer,
            IDeltaDownloader deltaDownloader,
            IMessageService messageService)
        {
            _updateService = updateService;
            _deltaAnalyzer = deltaAnalyzer;
            _deltaDownloader = deltaDownloader;
            _messageService = messageService;
        }

        public async Task CheckForUpdatesOnStartup()
        {
            try
            {
                await _messageService.SendAsync(
                    this,
                    UpdateEvents.StateChanged,
                    new UpdateEventArgs(UpdateState.Checking, "Güncellemeler kontrol ediliyor..."));

                var settings = await _updateService.GetUpdateSettings();

                // Önce bekleyen güncelleme var mı kontrol et
                await CheckForPendingUpdates(settings);

                if(!settings.AutoCheckOnStartup)
                {
                    await _messageService.SendAsync(
                        this,
                        UpdateEvents.StateChanged,
                        new UpdateEventArgs(UpdateState.Idle, "Otomatik kontrol kapalı"));
                    return;
                }

                if(!ShouldCheckForUpdates(settings))
                {
                    await _messageService.SendAsync(
                        this,
                        UpdateEvents.StateChanged,
                        new UpdateEventArgs(UpdateState.Idle, "Henüz kontrol zamanı gelmedi"));
                    return;
                }

                // Delta güncelleme kontrolü
                var deltaUpdateInfo = await _updateService.CheckForDeltaUpdateAsync();
                if(deltaUpdateInfo.IsDeltaAvailable)
                {
                    await ProcessDeltaUpdate(deltaUpdateInfo, settings);
                    return;
                }

                // Normal güncelleme kontrolü
                var updateInfo = await _updateService.CheckForUpdatesAsync();
                await _updateService.UpdateLastCheckDateAsync();

                if(updateInfo.HasError)
                {
                    await _messageService.SendAsync(
                        this,
                        UpdateEvents.Error,
                        new UpdateEventArgs(new Exception(updateInfo.ErrorMessage)));
                    return;
                }

                if(updateInfo.HasUpdate)
                {
                    // Mevcut güncellemeyi sakla
                    _currentAvailableUpdate = updateInfo;
                    await HandleRegularUpdate(updateInfo, settings);
                } else
                {
                    await _messageService.SendAsync(
                        this,
                        UpdateEvents.StateChanged,
                        new UpdateEventArgs(UpdateState.Idle, "Uygulama güncel"));
                }
            } catch(Exception ex)
            {
                await _messageService.SendAsync(this, UpdateEvents.Error, new UpdateEventArgs(ex));
            }
        }

        // Manuel güncelleme kontrolü - UI'dan çağrılabilir
        public async Task<UpdateInfo> CheckForUpdatesManually()
        {
            try
            {
                await _messageService.SendAsync(
                    this,
                    UpdateEvents.StateChanged,
                    new UpdateEventArgs(UpdateState.Checking, "Güncellemeler kontrol ediliyor..."));

                var settings = await _updateService.GetUpdateSettings();

                // Önce bekleyen güncelleme var mı kontrol et
                await CheckForPendingUpdates(settings);
                if(HasPendingUpdate)
                {
                    var state = !string.IsNullOrEmpty(PendingUpdateLocalPath)
                        ? UpdateState.Downloaded
                        : UpdateState.UpdateAvailable;
                    await _messageService.SendAsync(
                        this,
                        UpdateEvents.StateChanged,
                        new UpdateEventArgs(
                            state,
                            PendingUpdateInfo,
                            state == UpdateState.Downloaded ? "İndirilen güncelleme mevcut" : "Güncelleme mevcut"));

                    return PendingUpdateInfo;
                }

                // Delta güncelleme kontrolü
                var deltaUpdateInfo = await _updateService.CheckForDeltaUpdateAsync();
                if(deltaUpdateInfo.IsDeltaAvailable)
                {
                    await ProcessDeltaUpdate(deltaUpdateInfo, settings);
                    return _currentAvailableUpdate;
                }

                // Normal güncelleme kontrolü
                var updateInfo = await _updateService.CheckForUpdatesAsync();
                await _updateService.UpdateLastCheckDateAsync();

                if(updateInfo.HasError)
                {
                    await _messageService.SendAsync(
                        this,
                        UpdateEvents.Error,
                        new UpdateEventArgs(new Exception(updateInfo.ErrorMessage)));
                    return null;
                }

                if(updateInfo.HasUpdate)
                {
                    _currentAvailableUpdate = updateInfo;
                    await _messageService.SendAsync(
                        this,
                        UpdateEvents.StateChanged,
                        new UpdateEventArgs(UpdateState.UpdateAvailable, updateInfo, "Güncelleme mevcut"));

                    SetPendingUpdate(updateInfo);
                } else
                {
                    await _messageService.SendAsync(
                        this,
                        UpdateEvents.StateChanged,
                        new UpdateEventArgs(UpdateState.Idle, "Uygulama güncel"));
                }

                return updateInfo;
            } catch(Exception ex)
            {
                await _messageService.SendAsync(this, UpdateEvents.Error, new UpdateEventArgs(ex));
                return null;
            }
        }

        private async Task CheckForPendingUpdates(UpdateSettings settings)
        {
            try
            {
                var pendingInfo = await _updateService.GetPendingUpdateAsync();

                if(pendingInfo != null)
                {
                    // Dosya hala var mı ve hash doğru mu kontrol et
                    if(!string.IsNullOrEmpty(pendingInfo.LocalPath) && File.Exists(pendingInfo.LocalPath))
                    {
                        // Hash doğrulaması yap
                        var isValid = await _updateService.VerifyUpdateFileAsync(
                            pendingInfo.LocalPath,
                            pendingInfo.FileHash);

                        if(isValid)
                        {
                            SetPendingUpdate(pendingInfo.UpdateInfo, pendingInfo.LocalPath);
                            await _messageService.SendAsync(
                                this,
                                UpdateEvents.StateChanged,
                                new UpdateEventArgs(
                                    UpdateState.Downloaded,
                                    pendingInfo.UpdateInfo,
                                    "Güncelleme kurulmaya hazır"));
                            return;
                        } else
                        {
                            // Hash doğrulanamadı, dosya bozuk
                            await _messageService.SendAsync(
                                this,
                                UpdateEvents.Error,
                                new UpdateEventArgs(new Exception("İndirilen güncelleme dosyası bozuk")));
                        }
                    }

                    // Dosya bulunamadı veya bozuk, pending update'i temizle
                    await _updateService.ClearPendingUpdateAsync();
                }
            } catch(Exception ex)
            {
                Debug.WriteLine($"Pending update check failed: {ex.Message}");
                await _messageService.SendAsync(
                    this,
                    UpdateEvents.Error,
                    new UpdateEventArgs(ex, "Bekleyen güncelleme kontrolü başarısız"));
            }
        }

        public async Task ProcessDeltaUpdate(DeltaUpdateInfo deltaInfo, UpdateSettings settings)
        {
            try
            {
                var canApplyDelta = await _deltaAnalyzer.CanApplyDeltaUpdate(deltaInfo);

                if(!canApplyDelta)
                {
                    Debug.WriteLine("Delta update cannot be applied, falling back to regular update");
                    await FallbackToRegularUpdate(settings);
                    return;
                }

                var deltaUpdateInfo = new UpdateInfo
                {
                    HasUpdate = true,
                    LatestVersion = deltaInfo.NewVersion,
                    CurrentVersion = deltaInfo.CurrentVersion,
                    ReleaseNotes = $"Hızlı Delta Güncelleme - Sadece {deltaInfo.ChangedFilesCount} dosya değişti",
                    DownloadUrl = deltaInfo.DeltaDownloadUrl,
                    ChangelogUrl = deltaInfo.ChangelogUrl,
                    ReleaseNotesUrl = deltaInfo.ReleaseNotesUrl,
                };

                _currentAvailableUpdate = deltaUpdateInfo;

                await _messageService.SendAsync(
                    this,
                    UpdateEvents.StateChanged,
                    new UpdateEventArgs(UpdateState.UpdateAvailable, deltaUpdateInfo, "Delta güncelleme mevcut"));

                SetPendingUpdate(deltaUpdateInfo);

                if(settings.AutoDownload)
                {
                    await StartDeltaDownload(deltaInfo);
                }
            } catch(Exception ex)
            {
                Debug.WriteLine($"Delta update failed: {ex.Message}");
                await _messageService.SendAsync(
                    this,
                    UpdateEvents.Error,
                    new UpdateEventArgs(ex, "Delta güncelleme başarısız"));
                await FallbackToRegularUpdate(settings);
            }
        }

        private async Task StartDeltaDownload(DeltaUpdateInfo deltaInfo)
        {
            try
            {
                await _messageService.SendAsync(
                    this,
                    UpdateEvents.StateChanged,
                    new UpdateEventArgs(UpdateState.Downloading, "Delta güncelleme indiriliyor..."));

                var progress = new Progress<(long downloaded, long total, double speed)>(
                    p =>
                    {
                        _ = _messageService.SendAsync(
                            this,
                            UpdateEvents.Progress,
                            new UpdateProgressEventArgs(
                                    UpdateState.Downloading,
                                    p.downloaded,
                                    p.total,
                                    p.speed,
                                    "Delta güncelleme"));
                    });

                var success = await _deltaDownloader.DownloadDeltaUpdateAsync(deltaInfo, progress);

                if(success)
                {
                    await _messageService.SendAsync(
                        this,
                        UpdateEvents.StateChanged,
                        new UpdateEventArgs(
                            UpdateState.Installed,
                            "Delta güncelleme başarıyla uygulandı. Uygulama yeniden başlatılacak."));

                    await Task.Delay(2000);
                    Application.Current.Exit();
                } else
                {
                    throw new Exception("Delta güncelleme uygulaması başarısız");
                }
            } catch(Exception ex)
            {
                await _messageService.SendAsync(
                    this,
                    UpdateEvents.Error,
                    new UpdateEventArgs(ex, "Delta güncelleme başarısız"));
                await FallbackToRegularUpdate(await _updateService.GetUpdateSettings());
            }
        }

        private async Task FallbackToRegularUpdate(UpdateSettings settings)
        {
            try
            {
                var updateInfo = await _updateService.CheckForUpdatesAsync();
                if(updateInfo.HasUpdate)
                {
                    _currentAvailableUpdate = updateInfo;
                    await HandleRegularUpdate(updateInfo, settings);
                }
            } catch(Exception ex)
            {
                await _messageService.SendAsync(
                    this,
                    UpdateEvents.Error,
                    new UpdateEventArgs(ex, "Normal güncelleme kontrolü başarısız"));
            }
        }

        private async Task HandleRegularUpdate(UpdateInfo updateInfo, UpdateSettings settings)
        {
            await _messageService.SendAsync(
                this,
                UpdateEvents.StateChanged,
                new UpdateEventArgs(UpdateState.UpdateAvailable, updateInfo, "Güncelleme mevcut"));

            SetPendingUpdate(updateInfo);

            if(settings.AutoDownload)
            {
                await AutoDownloadUpdate(updateInfo);
            }
        }

        private async Task AutoDownloadUpdate(UpdateInfo updateInfo)
        {
            try
            {
                await _messageService.SendAsync(
                    this,
                    UpdateEvents.StateChanged,
                    new UpdateEventArgs(UpdateState.Downloading, "Otomatik indirme başladı"));

                var progress = new Progress<(long downloaded, long total, double speed)>(
                    p =>
                    {
                        _ = _messageService.SendAsync(
                            this,
                            UpdateEvents.Progress,
                            new UpdateProgressEventArgs(UpdateState.Downloading, p.downloaded, p.total, p.speed));
                    });

                var setupPath = await DownloadUpdateFile(updateInfo.DownloadUrl,expectedHash:null, progress);

                if(setupPath != null)
                {
                    // İndirme tamamlandı - Downloaded state'ine geç
                    SetPendingUpdate(updateInfo, setupPath);

                    await _messageService.SendAsync(
                        this,
                        UpdateEvents.StateChanged,
                        new UpdateEventArgs(UpdateState.Downloaded, updateInfo, "İndirme tamamlandı"));

                    // Pending update'i kaydet
                    await _updateService.SavePendingUpdateAsync(
                        new PendingUpdateInfo
                        {
                            UpdateInfo = updateInfo,
                            LocalPath = setupPath,
                            DownloadedAt = DateTime.Now
                        });
                } else
                {
                    await _messageService.SendAsync(
                        this,
                        UpdateEvents.Error,
                        new UpdateEventArgs(new Exception("Otomatik indirme başarısız")));
                }
            } catch(Exception ex)
            {
                await _messageService.SendAsync(
                    this,
                    UpdateEvents.Error,
                    new UpdateEventArgs(ex, "Otomatik indirme sırasında hata oluştu"));
            }
        }

        // Manuel indirme - daha güvenli hale getirildi
        public async Task DownloadUpdate(string downloadUrl = null)
        {
            try
            {
                UpdateInfo updateToDownload = null;
                string urlToUse = downloadUrl;
                string expectedHash = null;

                // Önce PendingUpdateInfo'yu kontrol et
                if (PendingUpdateInfo != null)
                {
                    updateToDownload = PendingUpdateInfo;
                    urlToUse = urlToUse ?? PendingUpdateInfo.DownloadUrl;
                    expectedHash = PendingUpdateInfo.FileHash;
                }
                else if (_currentAvailableUpdate != null)
                {
                    updateToDownload = _currentAvailableUpdate;
                    urlToUse = urlToUse ?? _currentAvailableUpdate.DownloadUrl;
                    expectedHash = _currentAvailableUpdate.FileHash;
                    SetPendingUpdate(_currentAvailableUpdate);
                }
                else
                {
                    await _messageService.SendAsync(
                        this,
                        UpdateEvents.StateChanged,
                        new UpdateEventArgs(UpdateState.Checking, "Güncelleme bilgisi alınıyor..."));

                    var updateInfo = await _updateService.CheckForUpdatesAsync();
                    if (updateInfo.HasUpdate)
                    {
                        updateToDownload = updateInfo;
                        urlToUse = urlToUse ?? updateInfo.DownloadUrl;
                        expectedHash = updateInfo.FileHash;
                        _currentAvailableUpdate = updateInfo;
                        SetPendingUpdate(updateInfo);
                    }
                    else
                    {
                        await _messageService.SendAsync(
                            this,
                            UpdateEvents.Error,
                            new UpdateEventArgs(new Exception("Mevcut güncelleme bulunamadı")));
                        return;
                    }
                }

                if (string.IsNullOrEmpty(urlToUse))
                {
                    await _messageService.SendAsync(
                        this,
                        UpdateEvents.Error,
                        new UpdateEventArgs(new Exception("İndirme URL'si bulunamadı")));
                    return;
                }

                // Zaten indirilmiş ve hash doğru mu kontrol et
                if (!string.IsNullOrEmpty(PendingUpdateLocalPath) && File.Exists(PendingUpdateLocalPath))
                {
                    if (!string.IsNullOrEmpty(expectedHash))
                    {
                        var isValid = await _updateService.VerifyUpdateFileAsync(PendingUpdateLocalPath, expectedHash);
                        if (isValid)
                        {
                            await _messageService.SendAsync(
                                this,
                                UpdateEvents.StateChanged,
                                new UpdateEventArgs(UpdateState.Downloaded, updateToDownload, "Güncelleme zaten indirilmiş"));
                            return;
                        }
                        else
                        {
                            // Hash doğrulanamadı, tekrar indir
                            await _messageService.SendAsync(
                                this,
                                UpdateEvents.StateChanged,
                                new UpdateEventArgs(UpdateState.Downloading, "Dosya bozuk, tekrar indiriliyor..."));
                        }
                    }
                }

                await _messageService.SendAsync(
                    this,
                    UpdateEvents.StateChanged,
                    new UpdateEventArgs(UpdateState.Downloading, "İndirme başladı"));

                var progress = new Progress<(long downloaded, long total, double speed)>(
                    p =>
                    {
                        _ = _messageService.SendAsync(
                            this,
                            UpdateEvents.Progress,
                            new UpdateProgressEventArgs(UpdateState.Downloading, p.downloaded, p.total, p.speed));
                    });

                // Hash bilgisi ile indirme yap
                var setupPath = await DownloadUpdateFile(urlToUse, expectedHash, progress);

                if (setupPath != null)
                {
                    SetPendingUpdate(updateToDownload, setupPath);

                    await _messageService.SendAsync(
                        this,
                        UpdateEvents.StateChanged,
                        new UpdateEventArgs(UpdateState.Downloaded, updateToDownload, "İndirme tamamlandı"));

                    // Pending update'i kaydet (hash bilgisi ile)
                    await _updateService.SavePendingUpdateAsync(
                        new PendingUpdateInfo
                        {
                            UpdateInfo = updateToDownload,
                            LocalPath = setupPath,
                            DownloadedAt = DateTime.Now,
                            FileSize = new FileInfo(setupPath).Length,
                            FileHash = await CalculateFileHashAsync(setupPath)
                        });
                }
            }
            catch (Exception ex)
            {
                await _messageService.SendAsync(
                    this,
                    UpdateEvents.Error,
                    new UpdateEventArgs(ex, $"İndirme hatası: {ex.Message}"));
            }
        }

        public async Task<string> DownloadUpdateFile(
            string downloadUrl,
            string expectedHash,
            IProgress<(long, long, double)> progress = null)
        {
            try
            {
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    throw new ArgumentException("İndirme URL'si boş olamaz", nameof(downloadUrl));
                }

                var progressCallback = progress ??
                    new Progress<(long downloaded, long total, double speed)>(
                        p =>
                        {
                            _ = _messageService.SendAsync(
                                this,
                                UpdateEvents.Progress,
                                new UpdateProgressEventArgs(UpdateState.Downloading, p.downloaded, p.total, p.speed));
                        });

                // Hash bilgisi ile indirme yap
                string filePath;
                if (!string.IsNullOrEmpty(expectedHash))
                {
                    filePath = await _updateService.DownloadUpdateFile(downloadUrl, expectedHash, progressCallback);
                }
                else
                {
                    filePath = await _updateService.DownloadUpdateFile(downloadUrl,expectedHash:null, progressCallback);
                }

                // Dosya integrity'sini kontrol et
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    if (fileInfo.Length == 0)
                    {
                        File.Delete(filePath);
                        throw new Exception("İndirilen dosya bozuk (dosya boyutu 0 byte)");
                    }
                }

                return filePath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DownloadUpdateFile hatası: {ex.Message}");
                await _messageService.SendAsync(
                    this,
                    UpdateEvents.Error,
                    new UpdateEventArgs(ex, $"Dosya indirme hatası: {ex.Message}"));
                throw;
            }
        }
        private async Task<string> CalculateFileHashAsync(string filePath)
        {
            try
            {
                using var stream = File.OpenRead(filePath);
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                var hash = await sha256.ComputeHashAsync(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Hash hesaplama hatası: {ex.Message}");
                return null;
            }
        }


        // Eski method - önce indir sonra kur
        public async Task DownloadAndInstallUpdate(string downloadUrl = null)
        {
            try
            {
                // Önce indir
                await DownloadUpdate(downloadUrl);

                // Sonra kurulum yap
                if(!string.IsNullOrEmpty(PendingUpdateLocalPath) && File.Exists(PendingUpdateLocalPath))
                {
                    await InstallPendingUpdate();
                } else
                {
                    await _messageService.SendAsync(
                        this,
                        UpdateEvents.Error,
                        new UpdateEventArgs(new Exception("İndirme tamamlandıktan sonra kurulum dosyası bulunamadı")));
                }
            } catch(Exception ex)
            {
                await _messageService.SendAsync(
                    this,
                    UpdateEvents.Error,
                    new UpdateEventArgs(ex, $"İndirme ve kurulum hatası: {ex.Message}"));
            }
        }

        public async Task InstallPendingUpdate()
        {
            if(!HasPendingUpdate || PendingUpdateInfo == null)
            {
                await _messageService.SendAsync(
                    this,
                    UpdateEvents.Error,
                    new UpdateEventArgs(new Exception("Kurulacak bekleyen güncelleme bulunamadı")));
                return;
            }

            try
            {
                if(!string.IsNullOrEmpty(PendingUpdateLocalPath) && File.Exists(PendingUpdateLocalPath))
                {
                    await InstallUpdate(PendingUpdateLocalPath);
                } else
                {                    
                    // Dosya bulunamazsa tekrar indir ve kur
                    await _messageService.SendAsync(
                        this,
                        UpdateEvents.StateChanged,
                        new UpdateEventArgs(
                            UpdateState.Downloading,
                            "Kurulum dosyası bulunamadı, tekrar indiriliyor..."));

                    await DownloadAndInstallUpdate(PendingUpdateInfo.DownloadUrl);
                }
            } catch(Exception ex)
            {               
                await _messageService.SendAsync(
                    this,
                    UpdateEvents.Error,
                    new UpdateEventArgs(ex, $"Güncelleme kurulumu başarısız: {ex.Message}"));
            }
        }

        private async Task InstallUpdate(string setupPath)
        {
            try
            {
                await _messageService.SendAsync(
                    this,
                    UpdateEvents.StateChanged,
                    new UpdateEventArgs(UpdateState.Installing, "Kurulum başlıyor..."));

                await InstallPackageWithPackageManager(setupPath);

                // Kurulum başarılı olursa temizle
                await _updateService.ClearPendingUpdateAsync();
                ClearPendingUpdate();
                if (File.Exists(setupPath))
                    File.Delete(setupPath);

                await _messageService.SendAsync(
                    this,
                    UpdateEvents.StateChanged,
                    new UpdateEventArgs(UpdateState.Installed, "Kurulum tamamlandı. Uygulama yeniden başlatılacak."));

                await Task.Delay(2000);
                Application.Current.Exit();
            } catch(Exception ex)
            {
                await _messageService.SendAsync(
                    this,
                    UpdateEvents.Error,
                    new UpdateEventArgs(ex, $"Kurulum hatası: {ex.Message}"));

                // DEĞIŞIKLIK: Debug mode'da dosyayı silme, sadece error state'ine geç
                if(System.Diagnostics.Debugger.IsAttached)
                {
                    await _messageService.SendAsync(
                        this,
                        UpdateEvents.StateChanged,
                        new UpdateEventArgs(
                            UpdateState.Downloaded,
                            PendingUpdateInfo,
                            "Debug modunda kurulum atlandı - dosya korundu"));
                } else
                {
                    // Production'da hatalı dosyayı temizle
                    try
                    {
                        if(File.Exists(setupPath))
                        {
                            File.Delete(setupPath);
                            await _updateService.ClearPendingUpdateAsync();
                            ClearPendingUpdate();
                        }
                    } catch(Exception deleteEx)
                    {
                        Debug.WriteLine($"Hatalı kurulum dosyası silinemedi: {deleteEx.Message}");
                    }
                }

                throw;
            }
        }

        private async Task InstallPackageWithPackageManager(string packagePath)
        {
            try
            {
                var packageManager = new PackageManager();
                var deploymentOperation = packageManager.AddPackageAsync(
                    new Uri(packagePath),
                    null,
                    DeploymentOptions.ForceApplicationShutdown);

                deploymentOperation.Progress = (operation, progress) =>
                {
                    _ = _messageService.SendAsync(
                        this,
                        UpdateEvents.Progress,
                        new UpdateProgressEventArgs(
                            UpdateState.Installing,
                            (long)progress.percentage,
                            100,
                            0,
                            "Kurulum yapılıyor..."));
                };

                var result = await deploymentOperation.AsTask();

                if(result.ExtendedErrorCode != null)
                {
                    throw new Exception($"Paket kurulum hatası: {result.ErrorText} (Kod: {result.ExtendedErrorCode})");
                }
            } catch(Exception ex)
            {
                Debug.WriteLine($"InstallPackageWithPackageManager hatası: {ex.Message}");
                throw new Exception($"Paket kurulumu başarısız: {ex.Message}", ex);
            }
        }

        private bool ShouldCheckForUpdates(UpdateSettings settings)
        {
            if(settings.LastCheckDate == null)
                return true;

            var timeSinceLastCheck = DateTime.Now - settings.LastCheckDate.Value;
            var checkInterval = TimeSpan.FromHours(settings.CheckIntervalHours);

            return timeSinceLastCheck >= checkInterval;
        }

        private async void SetPendingUpdate(UpdateInfo updateInfo, string localPath = null)
        {
            HasPendingUpdate = true;
            PendingUpdateInfo = updateInfo;
            PendingUpdateLocalPath = localPath;
            _currentAvailableUpdate = updateInfo;
            var state = string.IsNullOrEmpty(localPath) ? UpdateState.UpdateAvailable : UpdateState.Downloaded;

            await _messageService.SendAsync(
                this,
                UpdateEvents.PendingUpdateChanged,
                new UpdateEventArgs(state, updateInfo));
        }

        public async void ClearPendingUpdate()
        {
            HasPendingUpdate = false;
            PendingUpdateInfo = null;
            PendingUpdateLocalPath = null;
            _currentAvailableUpdate = null;

            await _messageService.SendAsync(
                this,
                UpdateEvents.PendingUpdateChanged,
                new UpdateEventArgs(UpdateState.Idle));
        }

        // Debug için ek metodlar
        public UpdateInfo GetCurrentAvailableUpdate() => _currentAvailableUpdate;

        public string GetDebugInfo()
        {
            return $"HasPendingUpdate: {HasPendingUpdate}, " +
                $"PendingUpdateInfo: {(PendingUpdateInfo != null ? "Set" : "Null")}, " +
                $"PendingUpdateLocalPath: {PendingUpdateLocalPath ?? "Null"}, " +
                $"CurrentAvailableUpdate: {(_currentAvailableUpdate != null ? "Set" : "Null")}";
        }
    }
}