using Muhasebe.Business.Helpers;
using Muhasebe.Business.Services.Abstract.Common;
using Muhasebe.Domain.Entities.Sistem;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;

namespace Muhasebe.Business.Services.Concreate.Common
{
    public class DeltaAnalyzer : IDeltaAnalyzer
    {
        private readonly string _currentAppPath;

        public DeltaAnalyzer()
        {
            _currentAppPath = AppDomain.CurrentDomain.BaseDirectory;
        }

        // Interface'den gelen metod - Delta uygulanabilir mi kontrol et
        public async Task<bool> CanApplyDeltaUpdate(DeltaUpdateInfo deltaInfo)
        {
            try
            {
                if (!deltaInfo.IsDeltaAvailable)
                    return false;

                // Disk alanı kontrolü
                var availableSpace = GetAvailableDiskSpace();
                if (availableSpace < deltaInfo.DeltaSize * 2) // 2x güvenlik payı
                    return false;

                // Dosya erişim kontrolü - kritik dosyalar kullanımda mı?
                var currentHashes = await GetCurrentFileHashesAsync();
                foreach (var changedFile in deltaInfo.ChangedFiles)
                {
                    var fullPath = Path.Combine(_currentAppPath, changedFile);
                    if (File.Exists(fullPath) && !CanAccessFile(fullPath))
                        return false;
                }

                return true;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delta analysis failed: {ex.Message}");
                return false;
            }
        }

        // Interface'den gelen metod - Değişen dosyaları al
        public async Task<List<string>> GetChangedFilesAsync(string fromVersion, string toVersion)
        {
            var currentHashes = await GetCurrentFileHashesAsync();
            var changedFiles = new List<string>();

            // Bu metod normalde server'dan iki versiyon arasındaki değişiklikleri almalı
            // Şimdilik mevcut implementasyonu kullanıyoruz
            return changedFiles;
        }

        // Mevcut uygulamanın dosya hash'lerini çıkar
        public async Task<ConcurrentDictionary<string, string>> GetCurrentFileHashesAsync()
        {
            var hashes = new ConcurrentDictionary<string, string>();
            var appFiles = Directory.GetFiles(_currentAppPath, "*.*", SearchOption.AllDirectories)
                .Where(f => !IsSystemFile(f) && !IsTemporaryFile(f))
                .ToArray();

            await Task.Run(() =>
            {
                Parallel.ForEach(appFiles, file =>
                {
                    try
                    {
                        var relativePath = Path.GetRelativePath(_currentAppPath, file);
                        var hash = CalculateFileHash(file);

                        lock (hashes)
                        {
                            hashes[relativePath] = hash;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Hash calculation failed for {file}: {ex.Message}");
                    }
                });
            });

            return hashes;
        }

        // GitHub release'den delta bilgisini parse et
        public DeltaUpdateInfo ParseDeltaInfo(string releaseBody, GitHubAsset[] assets)
        {
            try
            {
                var deltaInfo = new DeltaUpdateInfo();

                // Release body'den delta manifest JSON'ını çıkar
                var deltaJsonStart = releaseBody.IndexOf("```json");
                var deltaJsonEnd = releaseBody.IndexOf("```", deltaJsonStart + 7);

                if (deltaJsonStart != -1 && deltaJsonEnd != -1)
                {
                    var deltaJson = releaseBody.Substring(deltaJsonStart + 7, deltaJsonEnd - deltaJsonStart - 7).Trim();

                    try
                    {
                        var manifestData = JsonSerializer.Deserialize<Dictionary<string, object>>(deltaJson);

                        if (manifestData.ContainsKey("ChangedFiles") && manifestData["ChangedFiles"] is JsonElement changedFilesElement)
                        {
                            deltaInfo.ChangedFiles = JsonSerializer.Deserialize<string[]>(changedFilesElement.GetRawText()) ?? Array.Empty<string>();
                        }

                        if (manifestData.ContainsKey("FileHashes") && manifestData["FileHashes"] is JsonElement hashesElement)
                        {
                            deltaInfo.FileHashes = JsonSerializer.Deserialize<Dictionary<string, string>>(hashesElement.GetRawText()) ?? new Dictionary<string, string>();
                        }

                        if (manifestData.ContainsKey("Version") && manifestData["Version"] is JsonElement versionElement)
                        {
                            deltaInfo.NewVersion = versionElement.GetString();
                        }

                        if (manifestData.ContainsKey("PreviousVersion") && manifestData["PreviousVersion"] is JsonElement prevVersionElement)
                        {
                            deltaInfo.CurrentVersion = prevVersionElement.GetString();
                        }
                    }
                    catch (JsonException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to parse delta manifest JSON: {ex.Message}");
                    }
                }

                // Asset'lerden delta dosyasını bul
                var deltaAsset = assets?.FirstOrDefault(a =>
                    a.Name.Contains("delta") ||
                    a.Name.Contains("patch") ||
                    a.Name.EndsWith("_delta.zip"));

                if (deltaAsset != null)
                {
                    deltaInfo.DeltaDownloadUrl = deltaAsset.BrowserDownloadUrl;
                    deltaInfo.DeltaSize = deltaAsset.Size;
                    deltaInfo.IsDeltaAvailable = true;
                }

                // Full size'ı hesapla
                var fullAsset = assets?.FirstOrDefault(a => a.Name.EndsWith(".msix"));
                if (fullAsset != null)
                {
                    deltaInfo.FullSize = fullAsset.Size;
                }

                // Delta yüzdesini güncelle
                deltaInfo.ChangedFilesCount = deltaInfo.ChangedFiles?.Length ?? 0;

                return deltaInfo;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delta info parse failed: {ex.Message}");
                return new DeltaUpdateInfo { IsDeltaAvailable = false };
            }
        }

        // Hangi dosyaların değiştiğini analiz et
        public async Task<string[]> AnalyzeChangedFilesAsync(DeltaUpdateInfo deltaInfo)
        {
            if (!deltaInfo.IsDeltaAvailable || deltaInfo.FileHashes?.Count == 0)
                return Array.Empty<string>();

            var currentHashes = await GetCurrentFileHashesAsync();
            var changedFiles = new List<string>();

            foreach (var remoteFile in deltaInfo.FileHashes)
            {
                // Dosya yoksa veya hash farklıysa değişmiş
                if (!currentHashes.ContainsKey(remoteFile.Key) ||
                    currentHashes[remoteFile.Key] != remoteFile.Value)
                {
                    changedFiles.Add(remoteFile.Key);
                }
            }

            return changedFiles.ToArray();
        }

        private string CalculateFileHash(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(stream);
            return Convert.ToHexString(hashBytes);
        }

        private static bool IsSystemFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var excludeFiles = new[] { "app.config", "*.pdb", "*.log", "settings.json", "*.backup", "*.tmp" };

            return excludeFiles.Any(pattern =>
                pattern.Contains('*')
                    ? fileName.EndsWith(pattern.Replace("*", ""), StringComparison.OrdinalIgnoreCase)
                    : fileName.Equals(pattern, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsTemporaryFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            return fileName.StartsWith("~") || fileName.EndsWith(".tmp") || fileName.EndsWith(".temp");
        }

        private bool CanAccessFile(string filePath)
        {
            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private long GetAvailableDiskSpace()
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(_currentAppPath));
                return drive.AvailableFreeSpace;
            }
            catch
            {
                return long.MaxValue; // Hata durumunda devam et
            }
        }
    }
}
