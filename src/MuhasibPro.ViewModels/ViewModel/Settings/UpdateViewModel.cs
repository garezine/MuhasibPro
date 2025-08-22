using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Muhasebe.Business.Helpers;
using Muhasebe.Business.Models.UpdateModels;
using Muhasebe.Business.Services.Abstract.Update;
using Muhasebe.Domain.Entities.SistemDb;
using MuhasibPro.Core.Infrastructure.ViewModels;
using MuhasibPro.Core.Models.Update;
using MuhasibPro.Core.Services.Abstract.Common;
using MuhasibPro.Core.Services.Concreate.Update;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModel.Settings
{
    public partial class UpdateViewModel : ViewModelBase
    {
        private readonly IUpdateService _updateService;
        private readonly UpdateManager _updateManager;
        private readonly IMessageService _messageService;

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

        #region Core State Properties
        private UpdateState _currentState = UpdateState.Idle;
        public UpdateState CurrentState
        {
            get => _currentState;
            set
            {
                if (Set(ref _currentState, value))
                {
                    UpdateUIForState();
                }
            }
        }

        private UpdateInfo _currentUpdateInfo;
        public UpdateInfo CurrentUpdateInfo
        {
            get => _currentUpdateInfo;
            set
            {
                if (Set(ref _currentUpdateInfo, value))
                {
                    NotifyPropertyChanged(nameof(UpdateSize));
                    NotifyPropertyChanged(nameof(ReleaseDate));
                    NotifyPropertyChanged(nameof(UpdateInfoVisibility));
                    NotifyPropertyChanged(nameof(ChangelogUrl));
                    UpdateUIForState();
                }
            }
        }

        private int _progressPercentage;
        public int ProgressPercentage
        {
            get => _progressPercentage;
            set => Set(ref _progressPercentage, value);
        }

        private string _progressText = "";
        public string ProgressText
        {
            get => _progressText;
            set => Set(ref _progressText, value);
        }

        private string _progressDetails = "";
        public string ProgressDetails
        {
            get => _progressDetails;
            set => Set(ref _progressDetails, value);
        }
        #endregion

        #region UI Display Properties (Computed from State)
        public string StatusText => GetStatusTextForState();
        public string VersionText => GetVersionTextForState();
        public string UpdateButtonText => GetButtonTextForState();
        public bool IsUpdateButtonEnabled => GetButtonEnabledForState();
        public bool IsCheckButtonEnabled => CurrentState != UpdateState.Checking;
        //public Visibility UpdateBadgeVisibility => CurrentState == UpdateState.UpdateAvailable ? Visibility.Visible : Visibility.Collapsed;
        public Visibility UpdateDetailsVisibility => ShouldShowDetails() ? Visibility.Visible : Visibility.Collapsed;
        public Visibility UpdateInfoVisibility => CurrentUpdateInfo != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ProgressVisibility => IsProgressVisible() ? Visibility.Visible : Visibility.Collapsed;
        public Visibility UpdateCardVisibility => ShouldShowUpdateCard() ? Visibility.Visible : Visibility.Collapsed;
        public string StatusIconGlyph => GetStatusIconForState();
        public Brush StatusIconBrush => GetStatusIconBrushForState();
        // Update info display
        public string UpdateSize => CurrentUpdateInfo?.FormattedFileSize ?? "";
        public string ReleaseDate => CurrentUpdateInfo?.FormattedReleaseDate ?? "";
        public string ChangelogUrl
        {
            get
            {
                if (CurrentUpdateInfo == null) return "";

                // İlk önce ChangelogUrl'i kontrol et
                if (!string.IsNullOrEmpty(CurrentUpdateInfo.ChangelogUrl))
                    return CurrentUpdateInfo.ChangelogUrl;

                // Sonra ReleaseNotesUrl'i kontrol et
                if (!string.IsNullOrEmpty(CurrentUpdateInfo.ReleaseNotesUrl))
                    return CurrentUpdateInfo.ReleaseNotesUrl;

                return "";
            }
        }
        public Visibility ChangelogButtonVisibility => !string.IsNullOrEmpty(ChangelogUrl) ? Visibility.Visible : Visibility.Collapsed;
        #endregion

        #region Last Check Display
        private string _lastCheckText = "Son denetleme: Hiçbir zaman";
        public string LastCheckText
        {
            get => _lastCheckText;
            set => Set(ref _lastCheckText, value);
        }
        #endregion

        #region Error Handling
        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => Set(ref _errorMessage, value);
        }

        public Visibility ErrorVisibility => !string.IsNullOrEmpty(ErrorMessage) && CurrentState == UpdateState.Error ? Visibility.Visible : Visibility.Collapsed;
        #endregion

        #region Constructor
        public UpdateViewModel(IUpdateService updateService, UpdateManager updateManager,
                             IMessageService messageService, ICommonServices commonServices) : base(commonServices)
        {
            _updateService = updateService;
            _updateManager = updateManager;
            _messageService = messageService;

            // MessageService event'lerini dinle
            _messageService.Subscribe<UpdateManager, UpdateEventArgs>(this, OnUpdateStateChanged);
            _messageService.Subscribe<UpdateManager, UpdateProgressEventArgs>(this, OnUpdateProgress);
        }
        #endregion

        #region MessageService Event Handlers
        private void OnUpdateStateChanged(UpdateManager sender, string message, UpdateEventArgs args)
        {
            DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
            {
                switch (message)
                {
                    case UpdateEvents.StateChanged:
                        CurrentState = args.State;
                        CurrentUpdateInfo = args.UpdateInfo;
                        ErrorMessage = args.Error?.Message ?? "";
                        if (!string.IsNullOrEmpty(args.Message))
                        {
                            ProgressText = args.Message;
                        }
                        break;

                    case UpdateEvents.PendingUpdateChanged:
                        CurrentState = args.State;
                        CurrentUpdateInfo = args.UpdateInfo;
                        break;

                    case UpdateEvents.Error:
                        CurrentState = UpdateState.Error;
                        ErrorMessage = args.Error?.Message ?? "Bilinmeyen bir hata oluştu";
                        ProgressText = args.Message ?? "Bir hata oluştu";
                        break;
                }
            });
        }

        private void OnUpdateProgress(UpdateManager sender, string message, UpdateProgressEventArgs args)
        {
            DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
            {
                if (message == UpdateEvents.Progress)
                {
                    // Artık args.Percentage güvenli şekilde kullanılabilir
                    ProgressPercentage = args.Percentage;
                    ProgressText = args.Status ?? GetStatusTextForState();

                    if (args.Downloaded > 0 && args.Total > 0)
                    {
                        ProgressDetails = $"{args.FormattedDownloaded} / {args.FormattedTotal} • {args.FormattedSpeed}";
                    }

                    // Debug için
                    System.Diagnostics.Debug.WriteLine($"Progress: {args.Percentage}% ({args.FormattedDownloaded}/{args.FormattedTotal})");
                }
            });
        }
        #endregion

        #region State-based UI Logic
        private string GetStatusTextForState()
        {
            return CurrentState switch
            {
                UpdateState.Idle => "Güncelsiniz",
                UpdateState.Checking => "Güncelleştirmeler denetleniyor...",
                UpdateState.UpdateAvailable => "Bir güncelleştirme hazır",
                UpdateState.Downloading => "İndiriliyor...",
                UpdateState.Downloaded => "Yüklemeye hazır",
                UpdateState.Installing => "Yükleniyor...",
                UpdateState.Installed => "Yeniden başlatma bekleniyor",
                UpdateState.Error => "Bir sorun oluştu",
                _ => "Güncelsiniz"
            };
        }
        private bool ShouldShowUpdateCard()
        {
            return CurrentState switch
            {
                UpdateState.UpdateAvailable or
                UpdateState.Downloading or
                UpdateState.Downloaded or
                UpdateState.Installing or
                UpdateState.Installed or
                UpdateState.Error => true,
                _ => false,
            };
        }
        // YENİ EKLENDİ: Duruma göre uygun simgeyi döndürür
        private string GetStatusIconForState()
        {
            return CurrentState switch
            {
                UpdateState.Idle or UpdateState.Installed => "\uE930", // CheckmarkBold
                UpdateState.Checking or UpdateState.Downloading or UpdateState.Installing => "\uE895", // Sync
                UpdateState.UpdateAvailable or UpdateState.Downloaded => "\uE946", // Info
                UpdateState.Error => "\uE783", // Error
                _ => "\uE946", // Default to Info
            };
        }

        // YENİ EKLENDİ: Duruma göre uygun simge rengini döndürür
        private Brush GetStatusIconBrushForState()
        {
            var brushName = CurrentState switch
            {
                UpdateState.Idle or UpdateState.Installed => "SystemFillColorSuccessBrush",
                UpdateState.Error => "SystemFillColorCriticalBrush",
                _ => "SystemControlForegroundAccentBrush",
            };
            return (Brush)Application.Current.Resources[brushName];
        }

        private string GetVersionTextForState()
        {
            return CurrentState switch
            {
                UpdateState.UpdateAvailable when CurrentUpdateInfo != null =>
                    $"v{CurrentUpdateInfo.LatestVersion} hazır",
                UpdateState.Downloaded when CurrentUpdateInfo != null =>
                    $"v{CurrentUpdateInfo.LatestVersion} indirildi",
                // Error durumunda version text'te hata gösterme, sadece mevcut version'u göster
                UpdateState.Error => $"v{CurrentUpdateInfo.LatestVersion} {StatusText}",
                _ => $"Lütfen bekleyin..."
            };
        }

        private string GetButtonTextForState()
        {
            return CurrentState switch
            {
                UpdateState.Idle => "Kontrol Et",
                UpdateState.Checking => "Kontrol Ediliyor...",
                UpdateState.UpdateAvailable => "İndir",
                UpdateState.Downloading => "İndiriliyor...",
                UpdateState.Downloaded => "Yükle ve Yeniden Başlat",
                UpdateState.Installing => "Yükleniyor...",
                UpdateState.Error => "Tekrar Dene",
                UpdateState.Installed => "Yeniden Başlatılıyor...",
                _ => "Kontrol Et"
            };
        }

        private bool GetButtonEnabledForState()
        {
            return CurrentState switch
            {
                UpdateState.Checking => false,
                UpdateState.Downloading => false,
                UpdateState.Installing => false,
                UpdateState.Installed => false,
                _ => true
            };
        }

        private bool ShouldShowDetails()
        {
            return CurrentState == UpdateState.UpdateAvailable ||
                   CurrentState == UpdateState.Downloaded ||
                   CurrentState == UpdateState.Downloading;
        }

        private bool IsProgressVisible()
        {
            return CurrentState == UpdateState.Downloading ||
                   CurrentState == UpdateState.Installing;
        }

        private void UpdateUIForState()
        {
            // UI thread'de olduğumuzdan emin ol
            DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
            {
                // Tüm computed property'leri güncelle
                NotifyPropertyChanged(nameof(StatusText));
                NotifyPropertyChanged(nameof(VersionText));
                NotifyPropertyChanged(nameof(UpdateButtonText));
                NotifyPropertyChanged(nameof(IsUpdateButtonEnabled));
                
                NotifyPropertyChanged(nameof(UpdateDetailsVisibility));
                NotifyPropertyChanged(nameof(UpdateInfoVisibility));
                NotifyPropertyChanged(nameof(ProgressVisibility));
                NotifyPropertyChanged(nameof(ChangelogButtonVisibility));
                NotifyPropertyChanged(nameof(ErrorVisibility));

                NotifyPropertyChanged(nameof(UpdateCardVisibility));
                NotifyPropertyChanged(nameof(StatusIconGlyph));
                NotifyPropertyChanged(nameof(StatusIconBrush));
            });
        }
        #endregion

        #region Commands
        public ICommand UpdateActionCommand => new RelayCommand(async () => await UpdateActionAsync());
        public ICommand CheckNowCommand => new RelayCommand(async () => await CheckNowAsync());
        public ICommand OpenChangelogCommand => new RelayCommand(async () => await OpenChangelogAsync());

        // Settings commands
        public ICommand ToggleAutoCheckCommand => new RelayCommand(async () => await ToggleAutoCheckAsync());
        public ICommand ToggleAutoDownloadCommand => new RelayCommand(async () => await ToggleAutoDownloadAsync());
        public ICommand ToggleShowNotificationsCommand => new RelayCommand(async () => await ToggleShowNotificationsAsync());
        public ICommand ToggleIncludeBetaCommand => new RelayCommand(async () => await ToggleIncludeBetaAsync());
        #endregion

        #region Command Implementations
        private async Task UpdateActionAsync()
        {
            try
            {
                switch (CurrentState)
                {
                    case UpdateState.Idle:
                    case UpdateState.Error:
                        await CheckNowAsync();
                        break;

                    case UpdateState.UpdateAvailable:
                        if (CurrentUpdateInfo != null)
                        {
                            // Sadece indir - kurulum yapmaz
                            await _updateManager.DownloadUpdate(CurrentUpdateInfo.DownloadUrl);
                        }
                        break;

                    case UpdateState.Downloaded:
                        // Kurulumu başlat
                        await _updateManager.InstallPendingUpdate();
                        break;
                }
            }
            catch (Exception ex)
            {
                CurrentState = UpdateState.Error;
                ErrorMessage = ex.Message;
                ProgressText = "İşlem başarısız";
            }
        }

        private async Task CheckNowAsync()
        {
            try
            {
                // UpdateManager'ın CheckForUpdatesManually metodunu kullan
                var updateInfo = await _updateManager.CheckForUpdatesManually();
                UpdateLastCheckText();

                if (!string.IsNullOrEmpty(_updateManager.PendingUpdateLocalPath))
                {
                    CurrentState = UpdateState.Downloaded;
                    ProgressText = "Güncelleme kurulmaya hazır";
                }
                else
                {
                    CurrentState = UpdateState.UpdateAvailable;
                    ProgressText = "Güncelleme indirilebilir";
                }
            }
            catch (Exception ex)
            {
                CurrentState = UpdateState.Error;
                ErrorMessage = ex.Message;
                ProgressText = "Kontrol başarısız";
            }
        }

        private async Task OpenChangelogAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(ChangelogUrl))
                {
                    await Windows.System.Launcher.LaunchUriAsync(new Uri(ChangelogUrl));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Changelog açılamadı: {ex.Message}");
            }
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
                await _updateManager.CheckForUpdatesOnStartup();

                if (_updateManager.HasPendingUpdate)
                {
                    CurrentState = _updateManager.PendingUpdateLocalPath != null ?
                        UpdateState.Downloaded : UpdateState.UpdateAvailable;
                    CurrentUpdateInfo = _updateManager.PendingUpdateInfo;
                }

                UpdateLastCheckText();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Initial update check failed: {ex.Message}");
            }
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

        public void Unsubscribe()
        {
            _messageService.Unsubscribe<UpdateManager>(this);
            _messageService.Unsubscribe<UpdateManager, UpdateProgressEventArgs>(this);
        }
        #endregion
    }
}