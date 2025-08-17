using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Muhasebe.Business.Helpers;
using Muhasebe.Business.Services.Abstract.Common;
using Muhasebe.Domain.Entities.Sistem;
using MuhasibPro.Core.Infrastructure.Helpers;
using System.Diagnostics;
using Windows.Management.Deployment;

namespace MuhasibPro.Core.Services.Update
{
    public class UpdateManager
    {
        private readonly IDeltaAnalyzer _deltaAnalyzer;
        private readonly IDeltaDownloader _deltaDownloader;
        public IUpdateService _updateService;
        private ContentDialog _currentProgressDialog;

        public UpdateManager(IUpdateService updateService, IDeltaAnalyzer deltaAnalyzer, IDeltaDownloader deltaDownloader)
        {
            _updateService = updateService;
            _deltaAnalyzer = deltaAnalyzer;
            _deltaDownloader = deltaDownloader;
        }

        public async Task CheckForUpdatesOnStartup()
        {
            try
            {
                var settings = await _updateService.GetUpdateSettings();

                // Otomatik kontrol açık değilse çık
                if (!settings.AutoCheckOnStartup)
                    return;

                // Kontrol aralığı kontrolü
                if (!ShouldCheckForUpdates(settings))
                    return;

                // Önce delta güncelleme kontrolü yap
                var deltaUpdateInfo = await _updateService.CheckForDeltaUpdateAsync();

                if (deltaUpdateInfo.IsDeltaAvailable)
                {
                    await ProcessDeltaUpdate(deltaUpdateInfo, settings);
                    return;
                }

                // Delta yoksa normal güncelleme kontrolü
                var updateInfo = await _updateService.CheckForUpdatesAsync();
                await _updateService.UpdateLastCheckDateAsync();

                if (updateInfo.HasError)
                {
                    Debug.WriteLine($"Update check failed: {updateInfo.ErrorMessage}");
                    return;
                }

                if (updateInfo.HasUpdate)
                {
                    await HandleRegularUpdate(updateInfo, settings);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update check exception: {ex.Message}");
            }
        }

        private async Task HandleRegularUpdate(UpdateInfo updateInfo, UpdateSettings settings)
        {
            // Otomatik indirme varsa sessizce indir
            if (settings.AutoDownload)
            {
                await AutoDownloadUpdate(updateInfo, settings);
            }
            // Bildirim açıksa dialog göster
            else if (settings.ShowNotifications)
            {
                await ShowUpdateDialog(updateInfo);
            }
            // Hiçbiri açık değilse sadece pending olarak işaretle
            else
            {
                SetPendingUpdate(updateInfo);
            }
        }

        public async Task ProcessDeltaUpdate(DeltaUpdateInfo deltaInfo, UpdateSettings settings)
        {
            try
            {
                // Önce delta uygulanabilirliği kontrol et
                var canApplyDelta = await _deltaAnalyzer.CanApplyDeltaUpdate(deltaInfo);

                if (!canApplyDelta)
                {
                    Debug.WriteLine("Delta update cannot be applied, falling back to regular update");
                    await ShowErrorDialog("Delta güncelleme uygulanamıyor. Tam güncelleme yapılacak.");
                    await FallbackToRegularUpdate(settings);
                    return;
                }

                // Otomatik indirme kapalıysa ve bildirim açıksa kullanıcıya sor
                if (!settings.AutoDownload && settings.ShowNotifications)
                {
                    var confirmResult = await ShowDeltaUpdateConfirmDialog(deltaInfo);
                    if (!confirmResult)
                    {
                        // Kullanıcı delta güncellemeyi reddetti, normal güncelleme kontrolü yap
                        Debug.WriteLine("User declined delta update, falling back to regular update");
                        await FallbackToRegularUpdate(settings);
                        return;
                    }
                }
                else if (!settings.AutoDownload && !settings.ShowNotifications)
                {
                    // Sessiz mod - delta güncellemeyi pending olarak işaretle
                    SetPendingDeltaUpdate(deltaInfo);
                    return;
                }

                // Delta güncellemeyi başlat
                var progressDialog = await ShowProgressDialog("Delta güncelleme indiriliyor...");

                // İndirme ilerlemesini takip et
                var progress = new Progress<(long downloaded, long total, double speed)>(p =>
                {
                    var percentage = p.total > 0 ? (int)((double)p.downloaded / p.total * 100) : 0;
                    UpdateProgressDialog(progressDialog,
                        $"Delta güncelleme indiriliyor... {percentage}%",
                        $"{FormatBytes(p.downloaded)} / {FormatBytes(p.total)} • {FormatBytes((long)p.speed)}/s",
                        percentage);
                });

                var success = await _deltaDownloader.DownloadDeltaUpdateAsync(deltaInfo, progress);
                progressDialog.Hide();

                if (success)
                {
                    await ShowRestartDialog("Delta güncelleme başarıyla uygulandı. Değişikliklerin etkili olması için uygulamayı yeniden başlatın.");
                }
                else
                {
                    Debug.WriteLine("Delta update failed, falling back to regular update");
                    await ShowErrorDialog("Delta güncelleme uygulanamadı. Tam güncelleme yapılacak.");
                    await FallbackToRegularUpdate(settings);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Delta update failed: {ex.Message}");
                _currentProgressDialog?.Hide();
                await ShowErrorDialog($"Delta güncelleme hatası: {ex.Message}. Tam güncelleme yapılacak.");
                await FallbackToRegularUpdate(settings);
            }
        }

        private async Task<bool> ShowDeltaUpdateConfirmDialog(DeltaUpdateInfo deltaInfo)
        {
            var dialog = new ContentDialog
            {
                Title = "Hızlı Güncelleme Mevcut",
                Content = CreateDeltaUpdateContent(deltaInfo),
                PrimaryButtonText = "Hızlı Güncelle",
                SecondaryButtonText = "Tam Güncelle",
                CloseButtonText = "Daha Sonra",
                XamlRoot = WindowHelper.CurrentXamlRoot
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        private UIElement CreateDeltaUpdateContent(DeltaUpdateInfo deltaInfo)
        {
            var stackPanel = new StackPanel { Spacing = 10 };

            var titleText = new TextBlock
            {
                Text = "Hızlı Delta Güncelleme",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 16
            };
            stackPanel.Children.Add(titleText);

            var infoText = new TextBlock
            {
                Text = $"Yeni versiyon: {deltaInfo.NewVersion}\n" +
                       $"Sadece değişen dosyalar indirilecek\n" +
                       $"Boyut: {FormatBytes(deltaInfo.DeltaSize)} (Tam: {FormatBytes(deltaInfo.FullSize)})",
                TextWrapping = TextWrapping.Wrap
            };
            stackPanel.Children.Add(infoText);

            var benefitText = new TextBlock
            {
                Text = "✓ Daha hızlı indirme\n✓ Daha az veri kullanımı\n✓ Daha hızlı uygulama",
                Foreground = new SolidColorBrush(Colors.Green),
                FontSize = 12
            };
            stackPanel.Children.Add(benefitText);

            return stackPanel;
        }

        private async Task FallbackToRegularUpdate(UpdateSettings settings)
        {
            try
            {
                var updateInfo = await _updateService.CheckForUpdatesAsync();
                if (updateInfo.HasUpdate)
                {
                    await HandleRegularUpdate(updateInfo, settings);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fallback to regular update failed: {ex.Message}");
            }
        }

        private void SetPendingDeltaUpdate(DeltaUpdateInfo deltaInfo)
        {
            // Delta update'i pending olarak işaretle
            // Bu durumda normal UpdateInfo'ya çevirmemiz gerekebilir
            var updateInfo = new UpdateInfo
            {
                HasUpdate = true,
                LatestVersion = deltaInfo.NewVersion,
                CurrentVersion = deltaInfo.CurrentVersion,
                ReleaseNotes = $"Delta Güncelleme - Sadece {deltaInfo.ChangedFilesCount} dosya değişti",
                DownloadUrl = deltaInfo.DeltaDownloadUrl
            };

            SetPendingUpdate(updateInfo);
        }

        private async Task ShowRestartDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Güncelleme Tamamlandı",
                Content = message,
                PrimaryButtonText = "Yeniden Başlat",
                CloseButtonText = "Daha Sonra",
                XamlRoot = WindowHelper.CurrentXamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Application.Current.Exit();
            }
        }

        private string FormatBytes(long bytes)
        {
            if (bytes == 0) return "0 B";

            string[] suffixes = { "B", "KB", "MB", "GB" };
            int suffixIndex = 0;
            double size = bytes;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:F1} {suffixes[suffixIndex]}";
        }

        private void UpdateProgressDialog(ContentDialog dialog, string status, string details = "", int percentage = -1)
        {
            if (dialog?.Tag is ProgressDialogElements elements)
            {
                if (elements.StatusText != null)
                    elements.StatusText.Text = status;

                if (elements.DetailsText != null)
                    elements.DetailsText.Text = details;

                // Percentage verilmişse ProgressBar göster ve ProgressRing gizle
                if (percentage >= 0 && elements.ProgressBar != null && elements.ProgressRing != null)
                {
                    elements.ProgressRing.Visibility = Visibility.Collapsed;
                    elements.ProgressBar.Visibility = Visibility.Visible;
                    elements.ProgressBar.Value = percentage;
                }
                // Percentage yoksa ProgressRing göster
                else if (percentage < 0 && elements.ProgressBar != null && elements.ProgressRing != null)
                {
                    elements.ProgressRing.Visibility = Visibility.Visible;
                    elements.ProgressBar.Visibility = Visibility.Collapsed;
                }
            }
        }

        private bool ShouldCheckForUpdates(UpdateSettings settings)
        {
            if (settings.LastCheckDate == null)
                return true;

            var timeSinceLastCheck = DateTime.Now - settings.LastCheckDate.Value;
            var checkInterval = TimeSpan.FromHours(settings.CheckIntervalHours);

            return timeSinceLastCheck >= checkInterval;
        }

        public bool HasPendingUpdate { get; private set; }
        public UpdateInfo PendingUpdateInfo { get; private set; }

        private void SetPendingUpdate(UpdateInfo updateInfo)
        {
            HasPendingUpdate = true;
            PendingUpdateInfo = updateInfo;
            PendingUpdateChanged?.Invoke(updateInfo);
        }

        public async Task ShowUpdateDialog(UpdateInfo updateInfo)
        {
            var dialog = new ContentDialog
            {
                Title = "Güncelleme Mevcut",
                Content = CreateUpdateContent(updateInfo),
                PrimaryButtonText = "Güncelle",
                CloseButtonText = "Daha Sonra",
                XamlRoot = WindowHelper.CurrentXamlRoot,
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await DownloadAndInstallUpdate(updateInfo.DownloadUrl);
            }
            else
            {
                SetPendingUpdate(updateInfo);
            }
        }

        private UIElement CreateUpdateContent(UpdateInfo updateInfo)
        {
            var stackPanel = new StackPanel { Spacing = 10 };

            // Versiyon bilgisi
            var versionText = new TextBlock
            {
                Text = $"Mevcut versiyon: {updateInfo.CurrentVersion}\nYeni versiyon: {updateInfo.LatestVersion}",
                FontSize = 14
            };
            stackPanel.Children.Add(versionText);

            // Release notes (varsa)
            if (!string.IsNullOrEmpty(updateInfo.ReleaseNotes))
            {
                var notesHeader = new TextBlock
                {
                    Text = "Bu güncellemenin içeriği:",
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Margin = new Thickness(0, 10, 0, 5)
                };
                stackPanel.Children.Add(notesHeader);

                var notesScroll = new ScrollViewer
                {
                    MaxHeight = 150,
                    Content = new TextBlock
                    {
                        Text = updateInfo.ReleaseNotes,
                        TextWrapping = TextWrapping.Wrap,
                        IsTextSelectionEnabled = true
                    }
                };
                stackPanel.Children.Add(notesScroll);
            }

            return stackPanel;
        }

        public event Action<UpdateInfo> PendingUpdateChanged;

        public async Task DownloadAndInstallUpdate(string downloadUrl)
        {
            try
            {
                var progressDialog = await ShowProgressDialog("İndiriliyor...");

                var setupPath = await DownloadUpdateFile(downloadUrl, progressDialog);

                if (setupPath != null)
                {
                    progressDialog.Hide();
                    await InstallUpdate(setupPath);

                    // Pending update'i temizle
                    ClearPendingUpdate();
                }
            }
            catch (Exception ex)
            {
                _currentProgressDialog?.Hide();
                await ShowErrorDialog($"Güncelleme hatası: {ex.Message}");
            }
        }

        // Progress dialog elemanları için helper class
        private class ProgressDialogElements
        {
            public TextBlock StatusText { get; set; }
            public TextBlock DetailsText { get; set; }
            public ProgressRing ProgressRing { get; set; }
            public ProgressBar ProgressBar { get; set; }
        }

        public async Task<ContentDialog> ShowProgressDialog(string message = "İşlem devam ediyor...")
        {
            var progressRing = new ProgressRing
            {
                IsActive = true,
                Width = 50,
                Height = 50
            };

            var progressBar = new ProgressBar
            {
                Width = 300,
                Height = 4,
                Margin = new Thickness(0, 10, 0, 0),
                Visibility = Visibility.Collapsed // Başlangıçta gizli
            };

            var statusText = new TextBlock
            {
                Text = message,
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var detailsText = new TextBlock
            {
                Text = "",
                Margin = new Thickness(0, 5, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.Gray)
            };

            var stackPanel = new StackPanel
            {
                Children = { progressRing, progressBar, statusText, detailsText }
            };

            var dialog = new ContentDialog
            {
                Title = "Güncelleme",
                Content = stackPanel,
                SecondaryButtonText = "İptal",
                XamlRoot = WindowHelper.CurrentXamlRoot,
                IsPrimaryButtonEnabled = false
            };

            dialog.Tag = new ProgressDialogElements
            {
                StatusText = statusText,
                DetailsText = detailsText,
                ProgressRing = progressRing,
                ProgressBar = progressBar
            };

            _currentProgressDialog = dialog;

            // Dialog'u async olarak göster ama await etme
            _ = dialog.ShowAsync();

            // Kısa bir süre bekle ki dialog görünür olsun
            await Task.Delay(100);

            return dialog;
        }

        private async Task AutoDownloadUpdate(UpdateInfo updateInfo, UpdateSettings settings)
        {
            try
            {
                ContentDialog progressDialog = null;

                // Sadece bildirim açıksa progress göster
                if (settings.ShowNotifications)
                {
                    progressDialog = await ShowProgressDialog("Otomatik indiriliyor...");
                }

                var setupPath = await DownloadUpdateFile(updateInfo.DownloadUrl, progressDialog);

                if (progressDialog != null)
                {
                    progressDialog.Hide();
                }

                if (setupPath != null)
                {
                    // Bildirim açıksa kullanıcıya haber ver
                    if (settings.ShowNotifications)
                    {
                        await ShowDownloadCompletedDialog(updateInfo, setupPath);
                    }
                    else
                    {
                        // Sessizce pending olarak işaretle
                        SetPendingUpdate(updateInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Auto download failed: {ex.Message}");

                if (settings.ShowNotifications)
                {
                    await ShowErrorDialog($"Otomatik indirme başarısız: {ex.Message}");
                }
            }
        }

        private async Task ShowDownloadCompletedDialog(UpdateInfo updateInfo, string setupPath)
        {
            var dialog = new ContentDialog
            {
                Title = "İndirme Tamamlandı",
                Content = $"v{updateInfo.LatestVersion} indirildi.\n\nŞimdi kurulumu başlatmak istiyor musunuz?",
                PrimaryButtonText = "Kurulumu Başlat",
                CloseButtonText = "Daha Sonra",
                XamlRoot = WindowHelper.CurrentXamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await InstallUpdate(setupPath);
            }
            else
            {
                // Daha sonra için pending olarak işaretle
                SetPendingUpdate(updateInfo);
            }
        }

        public async Task<string> DownloadUpdateFile(string downloadUrl, ContentDialog progressDialog = null, IProgress<(long downloaded, long total, double speed)> progress = null)
        {
            try
            {
                UpdateProgressDialog(progressDialog, "İndirme başlatılıyor...");

                // Progress callback oluştur
                var progressCallback = progress ?? new Progress<(long downloaded, long total, double speed)>(p =>
                {
                    if (progressDialog != null)
                    {
                        var downloadedFormatted = FormatBytes(p.downloaded);
                        var totalFormatted = FormatBytes(p.total);
                        var speedFormatted = FormatBytes((long)p.speed);
                        var percentage = p.total > 0 ? (int)((double)p.downloaded / p.total * 100) : 0;

                        var statusText = $"İndiriliyor... {percentage}%";
                        var detailsText = $"{downloadedFormatted} / {totalFormatted} • {speedFormatted}/s";

                        UpdateProgressDialog(progressDialog, statusText, detailsText, percentage);
                    }
                });

                return await _updateService.DownloadUpdateFile(downloadUrl, progressCallback);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Download failed: {ex.Message}");
                progressDialog?.Hide();
                throw;
            }
        }
        private async Task InstallPackageWithPackageManager(string packagePath)
        {
            var packageManager = new PackageManager();
            var deploymentOperation = packageManager.AddPackageAsync(
                new Uri(packagePath),
                null,
                DeploymentOptions.ForceApplicationShutdown
            );

            // İlerlemeyi takip et
            deploymentOperation.Progress = (operation, progress) =>
            {
                Debug.WriteLine($"Kurulum ilerleme: {progress.percentage}%");
            };

            var result = await deploymentOperation.AsTask();

            if (result.ExtendedErrorCode != null)
            {
                throw new Exception($"Kurulum hatası: {result.ErrorText}");
            }
        }

        private async Task InstallUpdate(string setupPath)
        {
            var confirmDialog = new ContentDialog
            {
                Title = "Kurulum Hazır",
                Content = "Güncelleme indirildi. Şimdi kurulumu başlatmak istiyor musunuz?\n\nUygulama kapatılacak ve kurulum başlayacak.",
                PrimaryButtonText = "Kurulumu Başlat",
                CloseButtonText = "İptal",
                XamlRoot = WindowHelper.CurrentXamlRoot
            };

            var result = await confirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    await InstallPackageWithPackageManager(setupPath);
                    Application.Current.Exit();
                }
                catch (Exception ex)
                {
                    await ShowErrorDialog($"Kurulum başlatılamadı: {ex.Message}");
                }
            }
            else
            {
                // Kullanıcı iptal etti, temp dosyayı sil
                try
                {
                    if (File.Exists(setupPath))
                        File.Delete(setupPath);
                }
                catch
                {
                    // Silme hatası önemli değil
                }
            }
        }

        private async Task ShowErrorDialog(string message)
        {
            var errorDialog = new ContentDialog
            {
                Title = "Hata",
                Content = message,
                CloseButtonText = "Tamam",
                XamlRoot = WindowHelper.CurrentXamlRoot
            };

            await errorDialog.ShowAsync();
        }

        // Pending update'i temizlemek için
        public void ClearPendingUpdate()
        {
            HasPendingUpdate = false;
            PendingUpdateInfo = null;
            PendingUpdateChanged?.Invoke(null);
        }
    }
}