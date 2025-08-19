using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Muhasebe.Business.Helpers;
using Muhasebe.Business.Models;
using Muhasebe.Business.Services.Abstract.Update;
using Muhasebe.Domain.Entities.SistemDb;
using MuhasibPro.Core.Infrastructure.ViewModels;
using MuhasibPro.Core.Services.Abstract.Common;
using MuhasibPro.Core.Services.Concreate.Update;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModel.Settings
{
    public partial class UpdateViewModel : ViewModelBase
    {
        private readonly IUpdateService _updateService;
        private readonly UpdateManager _updateManager;

        #region Settings & Basic Properties
        private UpdateSettings _settings;
        public UpdateSettings Settings
        {
            get => _settings;
            set
            {
                if (Set(ref _settings, value))
                {
                    OnSettingsChanged();
                }
            }
        }

        private void OnSettingsChanged()
        {          
            NotifyPropertyChanged(nameof(IsAutoDownloadEnabled));
            NotifyPropertyChanged(nameof(IsNotificationEnabled));
        }

        public bool IsAutoDownloadEnabled => Settings?.AutoCheckOnStartup == true;
        public bool IsNotificationEnabled => Settings?.AutoCheckOnStartup == true;
        #endregion

        #region UI State Properties
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

        private bool _hasUpdate = false;
        public bool HasUpdate
        {
            get => _hasUpdate;
            set
            {
                if (Set(ref _hasUpdate, value))
                {
                    UpdateUIVisibility();
                }
            }
        }

        private Visibility _updateBadgeVisibility = Visibility.Collapsed;
        public Visibility UpdateBadgeVisibility
        {
            get => _updateBadgeVisibility;
            set => Set(ref _updateBadgeVisibility, value);
        }

        private Visibility _updateDetailsVisibility = Visibility.Collapsed;
        public Visibility UpdateDetailsVisibility
        {
            get => _updateDetailsVisibility;
            set => Set(ref _updateDetailsVisibility, value);
        }

        private Visibility _updateInfoVisibility = Visibility.Collapsed;
        public Visibility UpdateInfoVisibility
        {
            get => _updateInfoVisibility;
            set => Set(ref _updateInfoVisibility, value);
        }

        private Visibility _actionButtonsVisibility = Visibility.Collapsed;
        public Visibility ActionButtonsVisibility
        {
            get => _actionButtonsVisibility;
            set => Set(ref _actionButtonsVisibility, value);
        }
        #endregion

        #region Update Info Display Properties
        private string _updateDescription = "";
        public string UpdateDescription
        {
            get => _updateDescription;
            set => Set(ref _updateDescription, value);
        }

        private string _updateSize = "";
        public string UpdateSize
        {
            get => _updateSize;
            set => Set(ref _updateSize, value);
        }

        private string _releaseDate = "";
        public string ReleaseDate
        {
            get => _releaseDate;
            set => Set(ref _releaseDate, value);
        }

        private bool _isDownloadButtonEnabled = false;
        public bool IsDownloadButtonEnabled
        {
            get => _isDownloadButtonEnabled;
            set => Set(ref _isDownloadButtonEnabled, value);
        }

        private bool _isInstallButtonEnabled = false;
        public bool IsInstallButtonEnabled
        {
            get => _isInstallButtonEnabled;
            set => Set(ref _isInstallButtonEnabled, value);
        }

        private bool _isScheduleButtonEnabled = false;
        public bool IsScheduleButtonEnabled
        {
            get => _isScheduleButtonEnabled;
            set => Set(ref _isScheduleButtonEnabled, value);
        }
        #endregion

        #region Constructor
        public UpdateViewModel(IUpdateService updateService, UpdateManager updateManager, ICommonServices commonServices) : base(commonServices)
        {
            _updateService = updateService;
            _updateManager = updateManager;

            // UpdateManager event'lerini dinle
            _updateManager.PendingUpdateChanged += OnPendingUpdateChanged;
        }
        #endregion

        #region Initialization
        public async Task InitializeAsync()
        {
            await LoadUpdateSettings();
            await CheckInitialUpdateState();
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
     
        private async Task CheckInitialUpdateState()
        {
            try
            {
                // UpdateManager'ın startup check'ini kullan
                await _updateManager.CheckForUpdatesOnStartup();

                // Eğer pending update varsa UI'ı güncelle
                if (_updateManager.HasPendingUpdate)
                {
                    UpdateUIState(_updateManager.PendingUpdateInfo);
                }

                UpdateLastCheckText();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Initial update check failed: {ex.Message}");
            }
        }
        #endregion

        #region Commands
        public ICommand UpdateActionCommand => new RelayCommand(async () => await UpdateActionAsync());
        public ICommand CheckNowCommand => new RelayCommand(async () => await CheckNowAsync());
        public ICommand DownloadCommand => new RelayCommand(async () => await DownloadAsync());
        public ICommand InstallCommand => new RelayCommand(async () => await InstallAsync());
        public ICommand ScheduleCommand => new RelayCommand(async () => await ScheduleAsync());

        // Settings commands
        public ICommand ToggleAutoCheckCommand => new RelayCommand(async () => await ToggleAutoCheckAsync());
        public ICommand ToggleAutoDownloadCommand => new RelayCommand(async () => await ToggleAutoDownloadAsync());
        public ICommand ToggleShowNotificationsCommand => new RelayCommand(async () => await ToggleShowNotificationsAsync());
        public ICommand ToggleIncludeBetaCommand => new RelayCommand(async () => await ToggleIncludeBetaAsync());
        #endregion

        #region Command Implementations
        private async Task UpdateActionAsync()
        {
            if (HasUpdate && _updateManager.PendingUpdateInfo != null)
            {
                // UpdateManager'ın kendi dialog'unu kullan
                await _updateManager.DownloadAndInstallUpdate(_updateManager.PendingUpdateInfo.DownloadUrl);
                return;
            }

            await CheckNowAsync();
        }

        private async Task CheckNowAsync()
        {
            SetCheckingState();

            try
            {
                // UpdateManager'ın manuel check metodunu kullan
                var updateInfo = await _updateService.CheckForUpdatesAsync();
                await _updateService.UpdateLastCheckDateAsync();

                if (updateInfo.HasError)
                {
                    StatusText = "Kontrol edilemedi";
                    VersionText = updateInfo.ErrorMessage;
                }
                else if (updateInfo.HasUpdate)
                {
                    // UpdateManager'ın dialog'unu göster
                    await _updateManager.ShowUpdateDialog(updateInfo);
                }
                else
                {
                    // Manual check'te "güncel" mesajı göster
                    StatusText = "MuhasibPro güncel";
                }

                UpdateUIState(updateInfo);
                UpdateLastCheckText();
            }
            catch (Exception ex)
            {
                StatusText = "Kontrol edilemedi";
                VersionText = ex.Message;
            }
            finally
            {
                ResetCheckingState();
            }
        }

        private async Task DownloadAsync()
        {
            if (_updateManager.PendingUpdateInfo != null)
            {
                await _updateManager.DownloadAndInstallUpdate(_updateManager.PendingUpdateInfo.DownloadUrl);
            }
        }    
        private async Task InstallAsync()
        {
            // Bu durumda UpdateManager'da bir install metodu olması gerekiyor
            // Veya pending update'teki local path'i kullanabiliriz
            if (_updateManager.HasPendingUpdate)
            {
                await _updateManager.InstallPendingUpdate(); // Bunu eklemek gerekiyor
            }
        }

        private async Task ScheduleAsync()
        {
            // Sonraki başlangıçta yükle - pending update'i korumak yeterli
            _updateManager.ClearPendingUpdate();
            HasUpdate = false;
            await Task.CompletedTask;
        }
        #endregion

        #region UI State Management
        private void SetCheckingState()
        {
            IsUpdateButtonEnabled = false;
            IsCheckButtonEnabled = false;
            UpdateButtonText = "Kontrol Ediliyor...";
        }

        private void ResetCheckingState()
        {
            IsUpdateButtonEnabled = true;
            IsCheckButtonEnabled = true;
            UpdateButtonText = HasUpdate ? "Güncelle" : "Kontrol Et";
        }

        private void UpdateUIVisibility()
        {
            UpdateDetailsVisibility = HasUpdate ? Visibility.Visible : Visibility.Collapsed;
            UpdateBadgeVisibility = HasUpdate ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateUIState(UpdateInfo updateInfo)
        {
            if (updateInfo == null)
            {
                HasUpdate = false;
                return;
            }

            VersionText = ProcessInfoHelper.Version;

            if (updateInfo.HasError)
            {
                StatusText = "Kontrol edilemedi";
                VersionText = updateInfo.ErrorMessage;
                UpdateButtonText = "Tekrar Dene";
                HasUpdate = false;
            }
            else if (updateInfo.HasUpdate)
            {
                StatusText = "Güncelleme mevcut";
                VersionText = $"v{updateInfo.LatestVersion} hazır";
                UpdateButtonText = "Güncelle";
                HasUpdate = true;

                // Update details'i set et
                UpdateDescription = updateInfo.ReleaseNotes ?? "Yeni özellikler ve hata düzeltmeleri";
                UpdateSize = updateInfo.FormattedFileSize;
                ReleaseDate = updateInfo.FormattedReleaseDate;

                // Action buttons'ı göster
                UpdateInfoVisibility = Visibility.Visible;
                ActionButtonsVisibility = Visibility.Visible;

                // Button states - başlangıçta download enable
                IsDownloadButtonEnabled = true;
                IsInstallButtonEnabled = false;
                IsScheduleButtonEnabled = false;
            }
            else
            {
                StatusText = "MuhasibPro güncel";
                VersionText = $"En son sürüm: {updateInfo.CurrentVersion}";
                UpdateButtonText = "Kontrol Et";
                HasUpdate = false;
            }
        }

        private void OnPendingUpdateChanged(UpdateInfo updateInfo)
        {
            // UI thread'de çalıştır
            ContextService?.RunAsync(() =>
            {
                UpdateUIState(updateInfo);
            });
        }
        #endregion

        #region Settings Management
        private async Task ToggleAutoCheckAsync()
        {
           DispatcherQueue.GetForCurrentThread().TryEnqueue(async () =>
            {
                if (Settings != null)
                {
                    await SaveSettings();
                    OnSettingsChanged();

                    // Otomatik kontrol kapatıldığında bağımlı ayarları da kapat
                    if (!Settings.AutoCheckOnStartup)
                    {
                        Settings.AutoDownload = false;
                        Settings.ShowNotifications = false;
                        await SaveSettings();
                        OnSettingsChanged();
                    }
                }
            });
            await Task.CompletedTask;

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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Settings save error: {ex.Message}");
            }
        }
        #endregion

        #region Helper Methods
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
        #endregion
    }
}