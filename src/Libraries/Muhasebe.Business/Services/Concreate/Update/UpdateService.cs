using Muhasebe.Business.Helpers;
using Muhasebe.Business.Models.UpdateModels;
using Muhasebe.Business.Services.Abstract.Update;
using Muhasebe.Domain.Entities.SistemDb;
using Muhasebe.Domain.Interfaces.App;

using System.Diagnostics;
using System.Text.Json;

namespace Muhasebe.Business.Services.Concreate.Update
{
    public class UpdateService : IUpdateService, IDisposable
    {
        private readonly IUpdateSettingsRepository _updateSettingsRepository;
        private readonly IDeltaAnalyzer _deltaAnalyzer;
        private readonly IDeltaDownloader _deltaDownloader;
        private readonly HttpClient _httpClient;
        private readonly string _updateUrl;
        private readonly string _pendingUpdateFilePath;

        public UpdateService(
            IUpdateSettingsRepository updateSettingsRepository,
            IDeltaAnalyzer deltaAnalyzer,
            IDeltaDownloader deltaDownloader)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", ProcessInfoHelper.ProductName);
            _httpClient.Timeout = TimeSpan.FromMinutes(10); // Uzun sürebilecek indirmeler için
            _updateUrl = "https://api.github.com/repos/garezine/MuhasibPro/releases/latest";
            _updateSettingsRepository = updateSettingsRepository;
            _deltaAnalyzer = deltaAnalyzer;
            _deltaDownloader = deltaDownloader;

            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "MuhasibPro");
            Directory.CreateDirectory(appFolder);
            _pendingUpdateFilePath = Path.Combine(appFolder, "pending_update.json");
        }

        public async Task<PendingUpdateInfo> GetPendingUpdateAsync()
        {
            try
            {
                var settings = await GetUpdateSettings();

                if(string.IsNullOrEmpty(settings.PendingUpdateVersion))
                    return null;

                // Dosya kontrolü
                if(!string.IsNullOrEmpty(settings.PendingUpdateLocalPath) &&
                    File.Exists(settings.PendingUpdateLocalPath))
                {
                    var fileInfo = new FileInfo(settings.PendingUpdateLocalPath);
                    if(fileInfo.Length == settings.PendingUpdateFileSize)
                    {
                        return new PendingUpdateInfo
                        {
                            UpdateInfo =
                                new UpdateInfo
                                {
                                    LatestVersion = settings.PendingUpdateVersion,
                                    DownloadUrl = settings.PendingUpdateDownloadUrl,
                                    HasUpdate = true
                                },
                            LocalPath = settings.PendingUpdateLocalPath,
                            DownloadedAt = settings.PendingUpdateDownloadedAt ?? DateTime.Now,
                            FileSize = settings.PendingUpdateFileSize,
                            FileHash = settings.PendingUpdateFileHash
                        };
                    }
                }

                // Dosya bozuk, temizle
                await ClearPendingUpdateAsync();
                return null;
            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetPendingUpdateAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task SavePendingUpdateAsync(PendingUpdateInfo pendingInfo)
        {
            try
            {
                var settings = await GetUpdateSettings();

                if(!string.IsNullOrEmpty(pendingInfo.LocalPath) && File.Exists(pendingInfo.LocalPath))
                {
                    pendingInfo.FileSize = new FileInfo(pendingInfo.LocalPath).Length;
                    pendingInfo.FileHash = await CalculateFileHashAsync(pendingInfo.LocalPath);
                }

                settings.PendingUpdateVersion = pendingInfo.UpdateInfo?.LatestVersion;
                settings.PendingUpdateLocalPath = pendingInfo.LocalPath;
                settings.PendingUpdateDownloadUrl = pendingInfo.UpdateInfo?.DownloadUrl;
                settings.PendingUpdateDownloadedAt = pendingInfo.DownloadedAt;
                settings.PendingUpdateFileSize = pendingInfo.FileSize;
                settings.PendingUpdateFileHash = pendingInfo.FileHash;

                await SaveSettingsAsync(settings);
            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SavePendingUpdateAsync error: {ex.Message}");
            }
        }

        public async Task ClearPendingUpdateAsync()
        {
            try
            {
                var settings = await GetUpdateSettings();

                settings.PendingUpdateVersion = null;
                settings.PendingUpdateLocalPath = null;
                settings.PendingUpdateDownloadUrl = null;
                settings.PendingUpdateDownloadedAt = null;
                settings.PendingUpdateFileSize = 0;
                settings.PendingUpdateFileHash = null;

                await SaveSettingsAsync(settings);
            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ClearPendingUpdateAsync error: {ex.Message}");
            }
        }

        public async Task<bool> VerifyUpdateFileAsync(string filePath, string expectedHash)
        {
            try
            {
                if(string.IsNullOrEmpty(expectedHash) || !File.Exists(filePath))
                    return false;

                var actualHash = await CalculateFileHashAsync(filePath);
                return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VerifyUpdateFileAsync error: {ex.Message}");
                return false;
            }
        }

        private async Task<string> CalculateFileHashAsync(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = await sha256.ComputeHashAsync(stream);
            return Convert.ToHexString(hash);
        }

        public async Task<DeltaUpdateInfo> CheckForDeltaUpdateAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(_updateUrl);
                var release = Newtonsoft.Json.JsonConvert.DeserializeObject<GitHubRelease>(response);


                var deltaInfo = _deltaAnalyzer.ParseDeltaInfo(release.Body, release.Assets);

                if(deltaInfo.IsDeltaAvailable)
                {
                    deltaInfo.ChangedFiles = await _deltaAnalyzer.AnalyzeChangedFilesAsync(deltaInfo);
                }

                return deltaInfo;
            } catch(Exception)
            {
                return new DeltaUpdateInfo { IsDeltaAvailable = false };
            }
        }

        public async Task<UpdateInfo> CheckForUpdatesAsync()
        {
            try
            {
                var currentVersion = ProcessInfoHelper.GetVersion();
                var response = await _httpClient.GetStringAsync(_updateUrl);
                var release = Newtonsoft.Json.JsonConvert.DeserializeObject<GitHubRelease>(response);

                // Hem "v1.0.1" hem "v.1.0.1" formatını destekle
                var versionString = release.TagName.TrimStart('v').Replace("..", "."); // ".1.0.1" -> "1.0.1"

                if(versionString.StartsWith("."))
                    versionString = versionString.Substring(1); // ".1.0.1" -> "1.0.1"

                var latestVersion = Version.Parse(versionString);

                var downloadUrl = GetSetupUrl(release.Assets);
                if(string.IsNullOrEmpty(downloadUrl))
                {
                    return new UpdateInfo { HasError = true, ErrorMessage = "Güncelleme dosyası bulunamadı." };
                }               
                return new UpdateInfo
                {
                    HasUpdate = latestVersion > currentVersion,
                    CurrentVersion = currentVersion.ToString(),
                    LatestVersion = latestVersion.ToString(),
                    DownloadUrl = downloadUrl,
                    ReleaseNotes = release.Body ?? "Sürüm notları mevcut değil.",
                    FileSize = GetFileSize(release.Assets),
                    ReleaseDate = release.PublishedAt,
                    ChangelogUrl =release.HtmlUrl, // GitHub release sayfasının URL'si
                    ReleaseNotesUrl = release.HtmlUrl // Alternatif olarak aynı URL
                };
            } catch(Exception ex)
            {
                return new UpdateInfo { HasError = true, ErrorMessage = ex.Message };
            }
        }

        // Progress tracking ile ana download metodu
        public async Task<string> DownloadUpdateFile(
            string downloadUrl,
            string expectedHash,
            IProgress<(long downloaded, long total, double speed)> progress = null)
        {
            // Önce aynı hash'e sahip dosya var mı kontrol et
            var existingFile = FindFileByHash(expectedHash);
            if (existingFile != null)
            {
                Debug.WriteLine($"Update file already exists with same hash: {existingFile}");
                return existingFile;
            }

            // Eski dosyaları temizle (24 saatten eski)
            CleanOldUpdateFiles(TimeSpan.FromHours(24));

            // Versiyon bilgisini dosya adına ekle (opsiyonel)
            var versionPart = !string.IsNullOrEmpty(expectedHash)
                ? $"_{expectedHash.Substring(0, 8)}"
                : $"_{DateTime.Now:yyyyMMdd_HHmmss}";

            var fileName = $"MuhasibPro_Update{versionPart}.msix";
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
                using var fileStream = new FileStream(
                    tempPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    8192,
                    true);

                while (true)
                {
                    var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;

                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    downloadedBytes += bytesRead;

                    if (progress != null)
                    {
                        var now = DateTime.Now;
                        var elapsedMs = (now - lastProgressTime).TotalMilliseconds;

                        if (elapsedMs >= 100 || bytesRead == 0)
                        {
                            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                            var speed = elapsedSeconds > 0 ? downloadedBytes / elapsedSeconds : 0;

                            progress.Report((downloadedBytes, totalBytes, speed));
                            lastProgressTime = now;
                        }
                    }
                }

                if (progress != null)
                {
                    var finalSpeed = stopwatch.Elapsed.TotalSeconds > 0
                        ? downloadedBytes / stopwatch.Elapsed.TotalSeconds
                        : 0;
                    progress.Report((downloadedBytes, totalBytes, finalSpeed));
                }

                // İndirilen dosyanın hash'ini kontrol et
                if (!string.IsNullOrEmpty(expectedHash))
                {
                    var isValid = await VerifyFileHash(tempPath, expectedHash);
                    if (!isValid)
                    {
                        throw new Exception("Downloaded file hash does not match expected hash");
                    }
                }

                ValidateDownloadedFile(tempPath);

                return tempPath;
            }
            catch
            {
                if (File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch
                    {
                    }
                }
                throw;
            }
        }

        // Backward compatibility için eski metod
        public async Task<string> DownloadUpdateFile(string downloadUrl)
        { return await DownloadUpdateFile(downloadUrl, null); }

        #region Dosya Doğrulama
        private async Task ValidateDownloadFile(string downloadUrl)
        {
            var headRequest = new HttpRequestMessage(HttpMethod.Head, downloadUrl);
            using var headResponse = await _httpClient.SendAsync(headRequest);

            if(!headResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Güncelleme dosyası bulunamadı: {headResponse.StatusCode}");
            }

            // Content-Type kontrolü
            var contentType = headResponse.Content.Headers.ContentType?.MediaType;
            if(contentType != null &&
                contentType != "application/octet-stream" &&
                contentType != "application/x-msdownload" &&
                !contentType.Contains("executable"))
            {
                Debug.WriteLine($"Warning: Unexpected content type: {contentType}");
                // GitHub releases genelde application/octet-stream döndürür, ama zorunlu hata vermeyelim
            }
        }

        private static void ValidateDownloadedFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if(fileInfo.Length < 1024) // 1KB'dan küçükse muhtemelen hata sayfası
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
                if(buffer[0] != 0x4D || buffer[1] != 0x5A) // "MZ"
                {
                    Debug.WriteLine("Warning: Downloaded file may not be a valid Windows executable");
                }
            } catch
            {
                // PE header kontrolü başarısızsa devam et
            }
        }
        public async Task<bool> VerifyFileHash(string filePath, string expectedHash)
        {
            try
            {
                if (string.IsNullOrEmpty(expectedHash) || !File.Exists(filePath))
                    return false;

                var actualHash = await CalculateFileHashAsync(filePath);
                return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"VerifyFileHash error: {ex.Message}");
                return false;
            }
        }

        public string FindFileByHash(string expectedHash)
        {
            try
            {
                if (string.IsNullOrEmpty(expectedHash))
                    return null;

                var tempDir = Path.GetTempPath();
                var updateFiles = Directory.GetFiles(tempDir, "MuhasibPro_*.msix")
                    .Concat(Directory.GetFiles(tempDir, "MuhasibPro_*.exe"))
                    .ToArray();

                foreach (var file in updateFiles)
                {
                    try
                    {
                        var fileHash = CalculateFileHashAsync(file).GetAwaiter().GetResult();
                        if (string.Equals(fileHash, expectedHash, StringComparison.OrdinalIgnoreCase))
                        {
                            return file;
                        }
                    }
                    catch
                    {
                        // Hash hesaplama hatası durumunda bir sonraki dosyaya geç
                        continue;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FindFileByHash error: {ex.Message}");
                return null;
            }
        }

        public void CleanOldUpdateFiles(TimeSpan maxAge)
        {
            try
            {
                var tempDir = Path.GetTempPath();
                var updateFiles = Directory.GetFiles(tempDir, "MuhasibPro_*.msix")
                    .Concat(Directory.GetFiles(tempDir, "MuhasibPro_*.exe"))
                    .ToArray();

                foreach (var file in updateFiles)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        if (DateTime.Now - fileInfo.CreationTime > maxAge)
                        {
                            File.Delete(file);
                            Debug.WriteLine($"Deleted old update file: {file}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error deleting file {file}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CleanOldUpdateFiles error: {ex.Message}");
            }
        }

        #endregion

        public async Task<UpdateSettings> CheckForUpdatesOnSettings()
        {
            try
            {
                var shouldCheck = await _updateSettingsRepository.ShouldCheckForUpdatesAsync();
                if(!shouldCheck)
                {
                    Debug.WriteLine("Update check skipped due to settings");
                    return null;
                }

                var updateInfo = await CheckForUpdatesAsync();
                await _updateSettingsRepository.UpdateLastCheckDateAsync();

                if(updateInfo.HasError)
                {
                    Debug.WriteLine($"Update check failed: {updateInfo.ErrorMessage}");
                    return null;
                }

                if(updateInfo.HasUpdate)
                {
                    var settings = await _updateSettingsRepository.GetSettingsAsync();
                    return settings;
                }

                return null;
            } catch(Exception ex)
            {
                Debug.WriteLine($"Update check exception: {ex.Message}");
                return null;
            }
        }

        public async Task UpdateLastCheckDateAsync() { await _updateSettingsRepository.UpdateLastCheckDateAsync(); }

        private string GetSetupUrl(GitHubAsset[] assets)
        {
            if(assets == null || assets.Length == 0)
                return null;
            // Öncelikle .msix uzantılı dosyayı ara
            foreach(var asset in assets)
            {
                if(asset.Name.EndsWith(".msix", StringComparison.OrdinalIgnoreCase))
                    return asset.BrowserDownloadUrl;
            }
            // Öncelikle Setup.exe dosyasını ara
            foreach(var asset in assets)
            {
                if(asset.Name.EndsWith("Setup.exe", StringComparison.OrdinalIgnoreCase))
                {
                    return asset.BrowserDownloadUrl;
                }
            }

            // Setup.exe bulunamazsa .exe uzantılı ilk dosyayı ara
            foreach(var asset in assets)
            {
                if(asset.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    return asset.BrowserDownloadUrl;
                }
            }

            // Hiçbir exe bulunamazsa ilk asset'i döndür (fallback)
            return assets[0]?.BrowserDownloadUrl;
        }

        private long GetFileSize(GitHubAsset[] assets)
        {
            if(assets == null || assets.Length == 0)
                return 0;

            // Setup.exe dosyasının boyutunu bul
            foreach(var asset in assets)
            {
                if(asset.Name.EndsWith("Setup.exe", StringComparison.OrdinalIgnoreCase))
                {
                    return asset.Size;
                }
            }

            // Setup.exe bulunamazsa ilk exe dosyasının boyutunu döndür
            foreach(var asset in assets)
            {
                if(asset.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
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
            } catch(Exception ex)
            {
                Debug.WriteLine($"Get settings error: {ex.Message}");

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
            } catch(Exception ex)
            {
                Debug.WriteLine($"Settings save error: {ex.Message}");
                throw; // Settings kaydederken hata önemli, yukarı fırlat
            }
        }

        public void Dispose() { _httpClient?.Dispose(); }
    }
}