using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Muhasebe.Business.Helpers;
using Muhasebe.Business.Services.Abstract.Common;
using Muhasebe.Domain.Entities.Sistem;
using MuhasibPro.Core.Infrastructure.Update;
using MuhasibPro.Core.Infrastructure.ViewModels;
using MuhasibPro.Core.Services.Common;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModel.Settings
{
    public partial class UpdateViewModel : ViewModelBase
    {
        private readonly IUpdateService _updateService;
        private readonly UpdateManager _updateManager;
        private UpdateInfo _pendingUpdate;


        private UpdateSettings _settings;
        public UpdateSettings Settings
        {
            get => _settings;
            set
            {
                if (Set(ref _settings, value))
                {
                    // Settings değiştiğinde ilgili property'leri bildir
                    NotifyPropertyChanged(nameof(IsAutoDownloadEnabled));
                    NotifyPropertyChanged(nameof(IsNotificationEnabled));
                }
            }
        }

        // Enable/Disable property'leri ekle
        public bool IsAutoDownloadEnabled => Settings?.AutoCheckOnStartup == true;
        public bool IsNotificationEnabled => Settings?.AutoCheckOnStartup == true;

        private string _statusText = "MuhasibPro güncel";
        public string StatusText
        {
            get => _statusText;
            set => Set(ref _statusText, value);
        }


        private string _versionText = "";
        public string VersionText
        {
            get => _versionText;
            set => Set(ref _versionText, value);
        }

        private string _lastCheckText = "Son denetleme: Hiçbir zaman";
        public string LastCheckText
        {
            get => _lastCheckText;
            set => Set(ref _lastCheckText, value);
        }


        private string _updateButtonText = "Kontrol Et";
        public string UpdateButtonText
        {
            get => _updateButtonText;
            set => Set(ref _updateButtonText, value);
        }


        private bool _updateButtonIsAccent = false;
        public bool UpdateButtonIsAccent
        {
            get => _updateButtonIsAccent;
            set => Set(ref _updateButtonIsAccent, value);
        }


        private bool _hasUpdate = false;
        public bool HasUpdate
        {
            get => _hasUpdate;
            set => Set(ref _hasUpdate, value);
        }

        private bool _isUpdateButtonEnabled = true;
        public bool IsUpdateButtonEnabled
        {
            get => _isUpdateButtonEnabled;
            set => Set(ref _isUpdateButtonEnabled, value);
        }


        private bool _isCheckButtonEnabled = true;
        public bool IsCheckButtonEnabled
        {
            get => _isCheckButtonEnabled;
            set => Set(ref _isCheckButtonEnabled, value);
        }
        private Visibility _updateBadgeVisibility = Visibility.Collapsed;
        public Visibility UpdateBadgeVisibility
        {
            get => _updateBadgeVisibility;
            set => Set(ref _updateBadgeVisibility, value);
        }

        // İndirme durumu için yeni property'ler
        private bool _isDownloading = false;
        public bool IsDownloading
        {
            get => _isDownloading;
            set => Set(ref _isDownloading, value);
        }

        private int _downloadProgress = 0;
        public int DownloadProgress
        {
            get => _downloadProgress;
            set => Set(ref _downloadProgress, value);
        }

        public UpdateViewModel(IUpdateService updateService, UpdateManager updateManager, ICommonServices commonServices) : base(commonServices)
        {
            _updateService = updateService;
            _updateManager = updateManager;
            _updateManager.PendingUpdateChanged += OnPendingUpdateChanged;
        }

        public async Task InitializeAsync()
        {
            await LoadUpdateSettings();
            await CheckAndUpdateUIOnLoad();
        }

        private async Task LoadUpdateSettings()
        {
            try
            {
                Settings = await _updateService.GetUpdateSettings();
                UpdateLastCheckText();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Settings load error: {ex.Message}");
            }
        }

        private async Task CheckAndUpdateUIOnLoad()
        {
            try
            {
                // UpdateManager'ın startup check'ini kullan
                await _updateManager.CheckForUpdatesOnStartup();

                // UI state'ini güncelle
                if (_updateManager.HasPendingUpdate)
                {
                    await UpdateUIState(_updateManager.PendingUpdateInfo);
                }

                UpdateLastCheckText();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Silent update check failed: {ex.Message}");
            }
        }

        public ICommand UpdateActionCommand => new RelayCommand(async () => await UpdateActionAsync());
        private async Task UpdateActionAsync()
        {
            if (HasUpdate && _pendingUpdate != null)
            {
                await DownloadAndInstallUpdateDirect(_pendingUpdate.DownloadUrl);
                return;
            }

            await PerformUpdateCheck(isManualCheck: false);
        }
        public ICommand CheckNowCommand => new RelayCommand(async () => await CheckNowAsync());

        private async Task CheckNowAsync()
        {
            await PerformUpdateCheck(isManualCheck: true);
        }

        private async Task PerformUpdateCheck(bool isManualCheck)
        {
            IsUpdateButtonEnabled = false;
            IsCheckButtonEnabled = false;
            UpdateButtonText = "Kontrol Ediliyor...";

            try
            {
                // Önce delta güncelleme kontrolü
                var deltaUpdateInfo = await _updateService.CheckForDeltaUpdateAsync();
                if (deltaUpdateInfo.IsDeltaAvailable)
                {
                    await _updateManager.ProcessDeltaUpdate(deltaUpdateInfo, Settings);
                    return;
                }
                var updateInfo = await _updateService.CheckForUpdatesAsync();
                await _updateService.UpdateLastCheckDateAsync();

                if (isManualCheck)
                {
                    if (updateInfo.HasError)
                    {
                        await ShowErrorDialog($"Güncelleme kontrolü başarısız: {updateInfo.ErrorMessage}");
                    }
                    else if (updateInfo.HasUpdate)
                    {
                        await _updateManager.ShowUpdateDialog(updateInfo);
                    }
                    else
                    {
                        await ShowInfoDialog("En güncel sürümü kullanıyorsunuz!");
                    }
                }

                await UpdateUIState(updateInfo);
                UpdateLastCheckText();
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Güncelleme kontrolü sırasında hata: {ex.Message}");
            }
            finally
            {
                IsUpdateButtonEnabled = true;
                IsCheckButtonEnabled = true;
                UpdateButtonText = HasUpdate ? "Güncelle" : "Kontrol Et";
            }
        }

        private async Task DownloadAndInstallUpdateDirect(string downloadUrl)
        {
            var progressDialog = await _updateManager.ShowProgressDialog();

            try
            {
                var setupPath = await _updateManager.DownloadUpdateFile(downloadUrl, progressDialog);

                if (setupPath != null)
                {
                    progressDialog.Hide();
                    await InstallUpdate(setupPath);
                }
            }
            catch (Exception ex)
            {
                progressDialog.Hide();
                await ShowErrorDialog($"Güncelleme hatası: {ex.Message}");
            }
        }

        private async Task InstallUpdate(string setupPath)
        {
            try
            {
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = setupPath,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                System.Diagnostics.Process.Start(processInfo);
                Application.Current.Exit();
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Kurulum başlatılamadı: {ex.Message}");
            }
        }

        private void OnPendingUpdateChanged(UpdateInfo updateInfo)
        {
            Ioc.Default.GetService<Microsoft.UI.Dispatching.DispatcherQueue>()?.TryEnqueue(async () =>
            {
                await UpdateUIState(updateInfo);
            });
        }

        private async Task UpdateUIState(UpdateInfo updateInfo)
        {
            VersionText = ProcessInfoHelper.Version;
            if (updateInfo.HasError)
            {
                StatusText = "Kontrol edilemedi";
                VersionText = updateInfo.ErrorMessage;
                UpdateButtonText = "Tekrar Dene";
                UpdateButtonIsAccent = false;
                UpdateBadgeVisibility = Visibility.Collapsed;
                HasUpdate = false;
            }
            else if (updateInfo.HasUpdate)
            {
                StatusText = "Güncelleme mevcut";
                VersionText = $"v{updateInfo.LatestVersion} hazır";
                UpdateButtonText = "İndir ve Güncelle";
                UpdateButtonIsAccent = true;
                UpdateBadgeVisibility = Visibility.Visible;
                HasUpdate = true;
                _pendingUpdate = updateInfo;
            }
            else
            {
                StatusText = "MuhasibPro güncel";
                VersionText = $"En son sürüm: {updateInfo.CurrentVersion}";
                UpdateButtonText = "Kontrol Et";
                UpdateButtonIsAccent = false;
                UpdateBadgeVisibility = Visibility.Collapsed;
                HasUpdate = false;
            }
            await Task.CompletedTask;
        }

        private void UpdateLastCheckText()
        {
            if (Settings?.LastCheckDate != null)
            {
                var timeAgo = DateTime.Now - Settings.LastCheckDate.Value;
                string timeText;

                if (timeAgo.TotalMinutes < 1)
                    timeText = "Az önce";
                else if (timeAgo.TotalMinutes < 60)
                    timeText = $"{(int)timeAgo.TotalMinutes} dakika önce";
                else if (timeAgo.TotalHours < 24)
                    timeText = $"{(int)timeAgo.TotalHours} saat önce";
                else if (timeAgo.TotalDays < 7)
                    timeText = $"{(int)timeAgo.TotalDays} gün önce";
                else
                    timeText = Settings.LastCheckDate.Value.ToString("dd.MM.yyyy");

                LastCheckText = $"Son denetleme: {timeText}";
            }
            else
            {
                LastCheckText = "Son denetleme: Hiçbir zaman";
            }
        }

        public ICommand ToggleAutoCheckCommand => new RelayCommand(async () => await ToggleAutoCheckAsync());
        public ICommand ToggleAutoDownloadCommand => new RelayCommand(async () => await ToggleAutoDownloadAsync());
        public ICommand ToggleShowNotificationsCommand => new RelayCommand(async () => await ToggleShowNotificationsAsync());
        public ICommand ToggleIncludeBetaCommand => new RelayCommand(async () => await ToggleIncludeBetaAsync());

        // Settings değişiklik metodları - Düzeltilmiş
        private async Task ToggleAutoCheckAsync()
        {
            if (Settings != null)
            {
                await SaveSettings();

                // Enable/Disable property'lerini güncelle
                NotifyPropertyChanged(nameof(IsAutoDownloadEnabled));
                NotifyPropertyChanged(nameof(IsNotificationEnabled));

                // Eğer otomatik kontrol kapatıldıysa, bağımlı ayarları da kapat
                if (!Settings.AutoCheckOnStartup)
                {
                    Settings.AutoDownload = true;
                    Settings.ShowNotifications = true;
                    await SaveSettings();
                }
            }
        }

        private async Task ToggleAutoDownloadAsync()
        {
            if (Settings != null)
            {
                await SaveSettings();
            }
        }

        private async Task ToggleShowNotificationsAsync()
        {
            if (Settings != null)
            {
                await SaveSettings();
            }
        }

        private async Task ToggleIncludeBetaAsync()
        {
            if (Settings != null)
            {
                await SaveSettings();
            }
        }

        public async Task UpdateCheckInterval(int hours)
        {
            if (Settings != null)
            {
                Settings.CheckIntervalHours = hours;
                await SaveSettings();
            }
        }

        private async Task SaveSettings()
        {
            try
            {
                await _updateService.SaveSettingsAsync(Settings);
                NotifyPropertyChanged(nameof(IsAutoDownloadEnabled));
                NotifyPropertyChanged(nameof(IsNotificationEnabled));
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Ayarlar kaydedilirken hata oluştu: {ex.Message}");
            }
        }

        // Dialog metodları - bunlar View'de implement edilecek
        public event Func<string, Task> ErrorDialogRequested;
        public event Func<string, Task> InfoDialogRequested;

        private async Task ShowErrorDialog(string message)
        {
            if (ErrorDialogRequested != null)
                await ErrorDialogRequested(message);
        }

        private async Task ShowInfoDialog(string message)
        {
            if (InfoDialogRequested != null)
                await InfoDialogRequested(message);
        }
    }
}