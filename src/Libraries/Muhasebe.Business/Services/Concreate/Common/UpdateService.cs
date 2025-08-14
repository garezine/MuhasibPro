using Muhasebe.Business.Helpers;
using Muhasebe.Business.Services.Abstract.Common;
using Muhasebe.Domain.Entities.Sistem;
using Muhasebe.Domain.Interfaces.App;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Muhasebe.Business.Services.Concreate.Common
{
    public class UpdateService : IUpdateService, IDisposable
    {
        private readonly IUpdateSettingsRepository _updateSettingsRepository;
        private readonly IDeltaAnalyzer _deltaAnalyzer;
        private readonly IDeltaDownloader _deltaDownloader;
        private readonly HttpClient _httpClient;
        private readonly string _updateUrl;

        public UpdateService(IUpdateSettingsRepository updateSettingsRepository, IDeltaAnalyzer deltaAnalyzer, IDeltaDownloader deltaDownloader)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", ProcessInfoHelper.ProductName);
            _httpClient.Timeout = TimeSpan.FromMinutes(10); // Uzun sürebilecek indirmeler için
            _updateUrl = "https://api.github.com/repos/garezine/MuhasibPro/releases/latest";
            _updateSettingsRepository = updateSettingsRepository;
            _deltaAnalyzer = deltaAnalyzer;
            _deltaDownloader = deltaDownloader;
        }

        public async Task<DeltaUpdateInfo> CheckForDeltaUpdateAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(_updateUrl);
                var release = JsonConvert.DeserializeObject<GitHubRelease>(response);

               
                var deltaInfo = _deltaAnalyzer.ParseDeltaInfo(release.Body, release.Assets);

                if (deltaInfo.IsDeltaAvailable)
                {
                    deltaInfo.ChangedFiles = await _deltaAnalyzer.AnalyzeChangedFilesAsync(deltaInfo);
                }

                return deltaInfo;
            }
            catch (Exception ex)
            {
                return new DeltaUpdateInfo
                {
                    IsDeltaAvailable = false
                };
            }
        }
        public async Task<UpdateInfo> CheckForUpdatesAsync()
        {
            try
            {
                var currentVersion = ProcessInfoHelper.GetVersion();
                var response = await _httpClient.GetStringAsync(_updateUrl);
                var release = JsonConvert.DeserializeObject<GitHubRelease>(response);

                // Hem "v1.0.1" hem "v.1.0.1" formatını destekle
                var versionString = release.TagName
                    .TrimStart('v')
                    .Replace("..", "."); // ".1.0.1" -> "1.0.1"

                if (versionString.StartsWith("."))
                    versionString = versionString.Substring(1); // ".1.0.1" -> "1.0.1"

                var latestVersion = Version.Parse(versionString);

                var downloadUrl = GetSetupUrl(release.Assets);
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    return new UpdateInfo
                    {
                        HasError = true,
                        ErrorMessage = "Güncelleme dosyası bulunamadı."
                    };
                }

                return new UpdateInfo
                {
                    HasUpdate = latestVersion > currentVersion,
                    CurrentVersion = currentVersion.ToString(),
                    LatestVersion = latestVersion.ToString(),
                    DownloadUrl = downloadUrl,
                    ReleaseNotes = release.Body ?? "Sürüm notları mevcut değil.",
                    FileSize = GetFileSize(release.Assets),
                    ReleaseDate = release.PublishedAt
                };
            }
            catch (Exception ex)
            {
                return new UpdateInfo
                {
                    HasError = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        // Progress tracking ile ana download metodu
        public async Task<string> DownloadUpdateFile(string downloadUrl, IProgress<(long downloaded, long total, double speed)> progress = null)
        {
            var fileName = $"MuhasibPro_Update_{DateTime.Now:yyyyMMdd_HHmmss}.msix";
            var tempPath = Path.Combine(Path.GetTempPath(), fileName);

            try
            {
                // Önce HEAD request ile dosya kontrolü
                await ValidateDownloadFile(downloadUrl);

                using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                var downloadedBytes = 0L;
                var buffer = new byte[8192];
                var stopwatch = Stopwatch.StartNew();
                var lastProgressTime = DateTime.Now;

                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                while (true)
                {
                    var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    downloadedBytes += bytesRead;

                    // Progress bildirimi - 100ms'de bir veya %1 değişimde
                    if (progress != null)
                    {
                        var now = DateTime.Now;
                        var elapsedMs = (now - lastProgressTime).TotalMilliseconds;

                        if (elapsedMs >= 100 || bytesRead == 0) // Son chunk için de bildir
                        {
                            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                            var speed = elapsedSeconds > 0 ? downloadedBytes / elapsedSeconds : 0;

                            progress.Report((downloadedBytes, totalBytes, speed));
                            lastProgressTime = now;
                        }
                    }
                }

                // Son progress bildirimi
                if (progress != null)
                {
                    var finalSpeed = stopwatch.Elapsed.TotalSeconds > 0 ? downloadedBytes / stopwatch.Elapsed.TotalSeconds : 0;
                    progress.Report((downloadedBytes, totalBytes, finalSpeed));
                }

                // İndirilen dosyanın boyutunu kontrol et
                ValidateDownloadedFile(tempPath);

                return tempPath;
            }
            catch
            {
                // Hata durumunda temp dosyayı sil
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
                throw;
            }
        }

        // Backward compatibility için eski metod
        public async Task<string> DownloadUpdateFile(string downloadUrl)
        {
            return await DownloadUpdateFile(downloadUrl, null);
        }

        private async Task ValidateDownloadFile(string downloadUrl)
        {
            var headRequest = new HttpRequestMessage(HttpMethod.Head, downloadUrl);
            using var headResponse = await _httpClient.SendAsync(headRequest);

            if (!headResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Güncelleme dosyası bulunamadı: {headResponse.StatusCode}");
            }

            // Content-Type kontrolü
            var contentType = headResponse.Content.Headers.ContentType?.MediaType;
            if (contentType != null &&
                contentType != "application/octet-stream" &&
                contentType != "application/x-msdownload" &&
                !contentType.Contains("executable"))
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Unexpected content type: {contentType}");
                // GitHub releases genelde application/octet-stream döndürür, ama zorunlu hata vermeyelim
            }
        }

        private static void ValidateDownloadedFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length < 1024) // 1KB'dan küçükse muhtemelen hata sayfası
            {
                throw new Exception("İndirilen dosya çok küçük, geçerli bir setup dosyası değil.");
            }

            // PE header kontrolü (opsiyonel)
            try
            {
                using var fileStream = File.OpenRead(filePath);
                var buffer = new byte[2];
                fileStream.Read(buffer, 0, 2);

                // MZ header kontrolü (Windows executable)
                if (buffer[0] != 0x4D || buffer[1] != 0x5A) // "MZ"
                {
                    System.Diagnostics.Debug.WriteLine("Warning: Downloaded file may not be a valid Windows executable");
                }
            }
            catch
            {
                // PE header kontrolü başarısızsa devam et
            }
        }

        public async Task<UpdateSettings> CheckForUpdatesOnSettings()
        {
            try
            {
                var shouldCheck = await _updateSettingsRepository.ShouldCheckForUpdatesAsync();
                if (!shouldCheck)
                {
                    System.Diagnostics.Debug.WriteLine("Update check skipped due to settings");
                    return null;
                }

                var updateInfo = await CheckForUpdatesAsync();
                await _updateSettingsRepository.UpdateLastCheckDateAsync();

                if (updateInfo.HasError)
                {
                    System.Diagnostics.Debug.WriteLine($"Update check failed: {updateInfo.ErrorMessage}");
                    return null;
                }

                if (updateInfo.HasUpdate)
                {
                    var settings = await _updateSettingsRepository.GetSettingsAsync();
                    return settings;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update check exception: {ex.Message}");
                return null;
            }
        }

        public async Task UpdateLastCheckDateAsync()
        {
            await _updateSettingsRepository.UpdateLastCheckDateAsync();
        }

        private string GetSetupUrl(GitHubAsset[] assets)
        {
            if (assets == null || assets.Length == 0)
                return null;
            // Öncelikle .msix uzantılı dosyayı ara
            foreach (var asset in assets)
            {
                if (asset.Name.EndsWith(".msix", StringComparison.OrdinalIgnoreCase))
                    return asset.BrowserDownloadUrl;
            }
            // Öncelikle Setup.exe dosyasını ara
            foreach (var asset in assets)
            {
                if (asset.Name.EndsWith("Setup.exe", StringComparison.OrdinalIgnoreCase))
                {
                    return asset.BrowserDownloadUrl;
                }
            }

            // Setup.exe bulunamazsa .exe uzantılı ilk dosyayı ara
            foreach (var asset in assets)
            {
                if (asset.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    return asset.BrowserDownloadUrl;
                }
            }

            // Hiçbir exe bulunamazsa ilk asset'i döndür (fallback)
            return assets[0]?.BrowserDownloadUrl;
        }

        private long GetFileSize(GitHubAsset[] assets)
        {
            if (assets == null || assets.Length == 0)
                return 0;

            // Setup.exe dosyasının boyutunu bul
            foreach (var asset in assets)
            {
                if (asset.Name.EndsWith("Setup.exe", StringComparison.OrdinalIgnoreCase))
                {
                    return asset.Size;
                }
            }

            // Setup.exe bulunamazsa ilk exe dosyasının boyutunu döndür
            foreach (var asset in assets)
            {
                if (asset.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    return asset.Size;
                }
            }

            return assets[0]?.Size ?? 0;
        }

        public async Task<UpdateSettings> GetUpdateSettings()
        {
            try
            {
                return await _updateSettingsRepository.GetSettingsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get settings error: {ex.Message}");

                // Hata durumunda default settings döndür
                return new UpdateSettings
                {
                    AutoCheckOnStartup = true,
                    CheckIntervalHours = 24,
                    AutoDownload = false,
                    ShowNotifications = true,
                    IncludeBetaVersions = false,
                    LastCheckDate = null
                };
            }
        }

        public async Task SaveSettingsAsync(UpdateSettings updateSettings)
        {
            try
            {
                await _updateSettingsRepository.SaveSettingsAsync(updateSettings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Settings save error: {ex.Message}");
                throw; // Settings kaydederken hata önemli, yukarı fırlat
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }


 

}