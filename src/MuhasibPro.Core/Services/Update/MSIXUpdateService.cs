using Microsoft.UI.Xaml;
using Muhasebe.Business.Helpers;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace MuhasibPro.Core.Services.Update
{
    public class MSIXUpdateService
    {
        private readonly HttpClient _httpClient;
        private readonly string _githubApiUrl;
        private readonly string _tempUpdatePath;

        public MSIXUpdateService(string githubRepo)
        {
            _httpClient = new HttpClient();
            _githubApiUrl = $"https://api.github.com/repos/garezine/MuhasibPro/releases/latest";
            _tempUpdatePath = Path.Combine(Path.GetTempPath(), "MuhasibProUpdate");
        }

        // Mevcut UpdateInfo sınıfınızı genişletmek için
        public class MSIXUpdateInfo : UpdateInfo
        {
            public string MSIXDownloadUrl { get; set; }
            public string CertificateDownloadUrl { get; set; }
            public string InstallerDownloadUrl { get; set; }
            public bool RequiresCertificateInstall { get; set; }
            public bool IsSignedWithValidCert { get; set; }
        }

        public async Task<MSIXUpdateInfo> CheckForMSIXUpdatesAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(_githubApiUrl);
                var release = JsonSerializer.Deserialize<GitHubRelease>(response);

                var updateInfo = new MSIXUpdateInfo
                {
                    CurrentVersion = GetCurrentVersion(),
                    LatestVersion = release.TagName?.TrimStart('v'),
                    HasError = false
                };

                if (IsNewerVersion(updateInfo.CurrentVersion, updateInfo.LatestVersion))
                {
                    updateInfo.HasUpdate = true;

                    // GitHub release'den dosya URL'lerini al
                    foreach (var asset in release.Assets)
                    {
                        if (asset.Name.EndsWith(".msix"))
                            updateInfo.MSIXDownloadUrl = asset.BrowserDownloadUrl;
                        else if (asset.Name.EndsWith(".cer"))
                            updateInfo.CertificateDownloadUrl = asset.BrowserDownloadUrl;
                        else if (asset.Name.Contains("Installer.exe"))
                            updateInfo.InstallerDownloadUrl = asset.BrowserDownloadUrl;
                    }

                    // Sertifika durumunu kontrol et
                    updateInfo.RequiresCertificateInstall = await CheckIfCertificateRequired();
                }

                return updateInfo;
            }
            catch (Exception ex)
            {
                return new MSIXUpdateInfo
                {
                    HasError = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        private async Task<bool> CheckIfCertificateRequired()
        {
            try
            {
                // GitHub'dan sertifikayı indir ve kontrol et
                if (string.IsNullOrEmpty(_tempUpdatePath))
                    return true;

                // Sertifika zaten yüklü mü kontrol et
                return !IsCertificateInstalled("MuhasibPro"); // Subject name'inizi buraya yazın
            }
            catch
            {
                return true; // Şüpheli durumda sertifika gerekli say
            }
        }

        private bool IsCertificateInstalled(string subjectName)
        {
            try
            {
                using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);

                foreach (X509Certificate2 cert in store.Certificates)
                {
                    if (cert.Subject.Contains($"CN={subjectName}"))
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DownloadAndInstallMSIXAsync(MSIXUpdateInfo updateInfo,
            IProgress<int> progress = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Directory.CreateDirectory(_tempUpdatePath);

                // Yöntem seçimi: Kullanıcı deneyimine göre
                if (updateInfo.RequiresCertificateInstall)
                {
                    // Sertifika gerekiyorsa özel installer kullan
                    return await DownloadAndRunCustomInstaller(updateInfo, progress, cancellationToken);
                }
                else
                {
                    // Sertifika yüklüyse direkt MSIX kur
                    return await DownloadAndInstallMSIXDirect(updateInfo, progress, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MSIX install error: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> DownloadAndRunCustomInstaller(MSIXUpdateInfo updateInfo,
            IProgress<int> progress, CancellationToken cancellationToken)
        {
            try
            {
                string installerPath = Path.Combine(_tempUpdatePath, "MuhasibProInstaller.exe");

                // Özel installer'ı indir
                using var response = await _httpClient.GetAsync(updateInfo.InstallerDownloadUrl,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                using var fileStream = File.Create(installerPath);
                using var downloadStream = await response.Content.ReadAsStreamAsync();

                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                var buffer = new byte[8192];
                long downloadedBytes = 0;
                int bytesRead;

                while ((bytesRead = await downloadStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    downloadedBytes += bytesRead;

                    if (totalBytes > 0)
                        progress?.Report((int)(downloadedBytes * 100 / totalBytes));
                }

                // Installer'ı yönetici olarak çalıştır
                return await RunInstallerAsAdmin(installerPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Custom installer error: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> DownloadAndInstallMSIXDirect(MSIXUpdateInfo updateInfo,
            IProgress<int> progress, CancellationToken cancellationToken)
        {
            try
            {
                string msixPath = Path.Combine(_tempUpdatePath, "MuhasibPro.msix");

                // MSIX dosyasını indir
                using var response = await _httpClient.GetAsync(updateInfo.MSIXDownloadUrl,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                using var fileStream = File.Create(msixPath);
                using var downloadStream = await response.Content.ReadAsStreamAsync();

                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                var buffer = new byte[8192];
                long downloadedBytes = 0;
                int bytesRead;

                while ((bytesRead = await downloadStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    downloadedBytes += bytesRead;

                    if (totalBytes > 0)
                        progress?.Report((int)(downloadedBytes * 100 / totalBytes));
                }

                // MSIX'i PowerShell ile kur
                return await InstallMSIXPackage(msixPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MSIX direct install error: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> RunInstallerAsAdmin(string installerPath)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = installerPath,
                    UseShellExecute = true,
                    Verb = "runas", // Yönetici olarak çalıştır
                    Arguments = "/silent" // Sessiz kurulum
                };

                var process = Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();

                    // Kurulum başarılıysa uygulamayı kapat
                    if (process.ExitCode == 0)
                    {
                        // 2 saniye bekle sonra uygulamayı kapat
                        await Task.Delay(2000);
                        Application.Current.Exit();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Admin installer run error: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> InstallMSIXPackage(string msixPath)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"Add-AppxPackage -Path '{msixPath}'\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();

                    if (process.ExitCode == 0)
                    {
                        // Kurulum başarılı, uygulamayı yeniden başlat
                        await Task.Delay(1000);
                        RestartApplication();
                        return true;
                    }
                    else
                    {
                        var error = await process.StandardError.ReadToEndAsync();
                        Debug.WriteLine($"MSIX install error: {error}");
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MSIX PowerShell install error: {ex.Message}");
                return false;
            }
        }

        private void RestartApplication()
        {
            try
            {
                // UWP/MSIX uygulaması için yeniden başlatma
                var currentProcess = Process.GetCurrentProcess();
                var startInfo = new ProcessStartInfo
                {
                    FileName = "ms-windows-store://pdp/?ProductId=YourAppProductId", // Store ID'nizi buraya yazın
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                Application.Current.Exit();
            }
            catch
            {
                // Fallback: Normal çıkış
                Application.Current.Exit();
            }
        }

        private string GetCurrentVersion()
        {
            // Mevcut version alma metodunuzu kullanın
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        }

        private bool IsNewerVersion(string current, string latest)
        {
            try
            {
                var currentVersion = new Version(current);
                var latestVersion = new Version(latest);
                return latestVersion > currentVersion;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();

            // Geçici dosyaları temizle
            try
            {
                if (Directory.Exists(_tempUpdatePath))
                    Directory.Delete(_tempUpdatePath, true);
            }
            catch { }
        }
    }

  
 
}