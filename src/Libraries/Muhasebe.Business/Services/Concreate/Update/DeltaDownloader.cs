using Muhasebe.Business.Models;
using Muhasebe.Business.Services.Abstract.Update;
using System.IO.Compression;

namespace Muhasebe.Business.Services.Concreate.Common;
public class DeltaDownloader : IDeltaDownloader
{
    private readonly HttpClient _httpClient;
    private readonly string _backupDirectory;

    public DeltaDownloader()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MuhasibPro/1.0");
        _httpClient.Timeout = TimeSpan.FromMinutes(30); // 30 dakika timeout

        _backupDirectory = Path.Combine(Path.GetTempPath(), "MuhasibPro_Backup", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        Directory.CreateDirectory(_backupDirectory);
    }

    public async Task<bool> DownloadDeltaUpdateAsync(
        DeltaUpdateInfo deltaInfo,
        IProgress<(long downloaded, long total, double speed)> progress = null)
    {
        string tempDeltaPath = null;
        var backupFiles = new List<string>();

        try
        {
            if (!deltaInfo.IsDeltaAvailable || string.IsNullOrEmpty(deltaInfo.DeltaDownloadUrl))
                return false;

            tempDeltaPath = Path.Combine(Path.GetTempPath(), $"delta_update_{DateTime.Now:yyyyMMdd_HHmmss}.zip");

            // Delta paketini indir
            await DownloadFileAsync(deltaInfo.DeltaDownloadUrl, tempDeltaPath, progress);

            // Dosya bütünlüğünü kontrol et
            if (!await ValidateDeltaPackage(tempDeltaPath, deltaInfo))
            {
                throw new InvalidDataException("Delta paketi bozuk veya geçersiz");
            }

            // Delta paketini uygula
            backupFiles = await ApplyDeltaUpdateAsync(tempDeltaPath, deltaInfo.ChangedFiles);

            // Başarılı olduysa backup'ları temizle
            CleanupBackupFiles(backupFiles);

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Delta update failed: {ex.Message}");

            // Hata durumunda rollback yap
            if (backupFiles.Count > 0)
            {
                await RollbackChanges(backupFiles);
            }

            return false;
        }
        finally
        {
            // Temp dosyayı sil
            if (tempDeltaPath != null && File.Exists(tempDeltaPath))
            {
                try { File.Delete(tempDeltaPath); } catch { }
            }
        }
    }

    private async Task<bool> ValidateDeltaPackage(string packagePath, DeltaUpdateInfo deltaInfo)
    {
        try
        {
            using var archive = ZipFile.OpenRead(packagePath);

            // Manifest dosyasını kontrol et
            var manifestEntry = archive.GetEntry("delta_manifest.json");
            if (manifestEntry == null)
                return await Task.FromResult(false);

            // Beklenen dosyaların varlığını kontrol et
            foreach (var expectedFile in deltaInfo.ChangedFiles)
            {
                if (archive.GetEntry(expectedFile) == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Expected file not found in delta package: {expectedFile}");
                    return await Task.FromResult(false);
                }
            }

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Delta package validation failed: {ex.Message}");
            return await Task.FromResult(false);
        }
    }

    private async Task DownloadFileAsync(string url, string localPath, IProgress<(long, long, double)> progress)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? 0;
        var downloadedBytes = 0L;
        var buffer = new byte[8192];
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var lastReportTime = DateTime.Now;

        using var contentStream = await response.Content.ReadAsStreamAsync();
        using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        while (true)
        {
            var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0) break;

            await fileStream.WriteAsync(buffer, 0, bytesRead);
            downloadedBytes += bytesRead;

            // Progress raporu (her saniyede bir)
            if (progress != null && (DateTime.Now - lastReportTime).TotalSeconds >= 1)
            {
                var speed = stopwatch.Elapsed.TotalSeconds > 0 ? downloadedBytes / stopwatch.Elapsed.TotalSeconds : 0;
                progress.Report((downloadedBytes, totalBytes, speed));
                lastReportTime = DateTime.Now;
            }
        }

        // Son progress raporu
        if (progress != null)
        {
            var finalSpeed = stopwatch.Elapsed.TotalSeconds > 0 ? downloadedBytes / stopwatch.Elapsed.TotalSeconds : 0;
            progress.Report((downloadedBytes, totalBytes, finalSpeed));
        }
    }

    private async Task<List<string>> ApplyDeltaUpdateAsync(string deltaPackagePath, string[] changedFiles)
    {
        var backupFiles = new List<string>();
        var appPath = AppDomain.CurrentDomain.BaseDirectory;

        using var archive = ZipFile.OpenRead(deltaPackagePath);

        foreach (var entry in archive.Entries)
        {
            if (entry.FullName == "delta_manifest.json") // Manifest dosyasını atla
                continue;

            if (changedFiles.Contains(entry.FullName))
            {
                var destinationPath = Path.Combine(appPath, entry.FullName);
                var destinationDir = Path.GetDirectoryName(destinationPath);

                if (!Directory.Exists(destinationDir))
                    Directory.CreateDirectory(destinationDir);

                // Backup oluştur
                if (File.Exists(destinationPath))
                {
                    var backupFileName = $"{Path.GetFileName(destinationPath)}.backup";
                    var backupPath = Path.Combine(_backupDirectory, entry.FullName.Replace('/', '_').Replace('\\', '_') + ".backup");

                    Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
                    File.Copy(destinationPath, backupPath, true);
                    backupFiles.Add($"{destinationPath}|{backupPath}");
                }

                // Yeni dosyayı çıkar
                entry.ExtractToFile(destinationPath, true);

                // Dosya izinlerini ayarla
                File.SetAttributes(destinationPath, FileAttributes.Normal);
            }
        }

        return await Task.FromResult(backupFiles);
    }

    private async Task RollbackChanges(List<string> backupFiles)
    {
        foreach (var backupInfo in backupFiles)
        {
            try
            {
                var parts = backupInfo.Split('|');
                if (parts.Length == 2)
                {
                    var originalPath = parts[0];
                    var backupPath = parts[1];

                    if (File.Exists(backupPath))
                    {
                        File.Copy(backupPath, originalPath, true);
                        System.Diagnostics.Debug.WriteLine($"Rolled back: {originalPath}");
                    }
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Rollback failed for {backupInfo}: {ex.Message}");
            }
        }
    }

    private void CleanupBackupFiles(List<string> backupFiles)
    {
        foreach (var backupInfo in backupFiles)
        {
            try
            {
                var backupPath = backupInfo.Split('|')[1];
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
            }
            catch
            {
                // Cleanup hatalarını yoksay
            }
        }

        // Backup dizinini temizle
        try
        {
            if (Directory.Exists(_backupDirectory) && !Directory.EnumerateFileSystemEntries(_backupDirectory).Any())
            {
                Directory.Delete(_backupDirectory, true);
            }
        }
        catch
        {
            // Cleanup hatalarını yoksay
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
