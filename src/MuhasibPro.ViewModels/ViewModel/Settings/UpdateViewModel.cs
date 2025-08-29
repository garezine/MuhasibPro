using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using MuhasibPro.Core.Infrastructure.ViewModels;
using MuhasibPro.Core.Models.Update;
using MuhasibPro.Core.Services.Abstract.Common;

namespace MuhasibPro.ViewModels.ViewModel.Settings
{
    public enum UpdateState
    {
        Idle,           // Güncel
        Checking,       // Kontrol ediliyor
        UpdateAvailable, // Güncelleme mevcut
        Downloading,    // İndiriliyor
        Downloaded,     // İndirildi, kuruluma hazır
        Installing,     // Kuruluyor
        RestartRequired, // Yeniden başlatma gerekli
        Error           // Hata
    }

    public partial class UpdateViewModel : ViewModelBase
    {
        private readonly IUpdateService _updateService;
        private UpdateSettings _settings;
        private Velopack.UpdateInfo? _currentUpdateInfo;

        #region Properties

        private UpdateState _currentState = UpdateState.Idle;
        public UpdateState CurrentState
        {
            get => _currentState;
            set
            {
                if (Set(ref _currentState, value))
                {
                    UpdateUIProperties();
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

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (Set(ref _errorMessage, value))
                {
                    NotifyPropertyChanged(nameof(ErrorVisibility));
                }
            }
        }

        private string _lastCheckText = "Son denetleme: Hiçbir zaman";
        public string LastCheckText
        {
            get => _lastCheckText;
            set => Set(ref _lastCheckText, value);
        }

        // Settings binding - eski XAML ile uyumlu
        public UpdateSettings Settings
        {
            get => _settings;
            set
            {
                if (Set(ref _settings, value))
                {
                    NotifyPropertyChanged(nameof(IsAutoDownloadEnabled));
                    NotifyPropertyChanged(nameof(IsNotificationEnabled));
                }
            }
        }

        // UI States - eski XAML bindingler
        public string StatusText => GetStatusText();
        public string VersionText => GetVersionText();
        public string UpdateButtonText => GetButtonText();
        public bool IsUpdateButtonEnabled => GetButtonEnabled();
        public bool IsCheckButtonEnabled => CurrentState != UpdateState.Checking;

        // Visibilities - eski XAML bindingler
        public Visibility UpdateCardVisibility => ShouldShowUpdateCard() ? Visibility.Visible : Visibility.Collapsed;
        public Visibility UpdateDetailsVisibility => ShouldShowDetails() ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ProgressVisibility => IsProgressVisible() ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ErrorVisibility => !string.IsNullOrEmpty(ErrorMessage) && CurrentState == UpdateState.Error ? Visibility.Visible : Visibility.Collapsed;

        // Icon and Colors - eski XAML bindingler
        public string StatusIconGlyph => GetStatusIcon();
        public Brush StatusIconBrush => GetStatusIconBrush();

        // Update Info - eski XAML binding isimleri korundu
        public string UpdateSize => _currentUpdateInfo?.TargetFullRelease.Size.ToString("N0") + " bytes" ?? "";
        public string ReleaseDate => _currentUpdateInfo?.TargetFullRelease.Version.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("tr-TR")) ?? "";

        // Eski XAML'deki ChangelogUrl binding
        public string ChangelogUrl => _currentUpdateInfo != null ?
            $"https://github.com/garezine/MuhasibPro/releases/tag/v{_currentUpdateInfo.TargetFullRelease.Version}" : "";
        public Visibility ChangelogButtonVisibility => !string.IsNullOrEmpty(ChangelogUrl) ? Visibility.Visible : Visibility.Collapsed;

        // Eski XAML'deki UpdateInfoVisibility binding
        public Visibility UpdateInfoVisibility => _currentUpdateInfo != null ? Visibility.Visible : Visibility.Collapsed;

        // Settings Dependencies - eski XAML bindingler
        public bool IsAutoDownloadEnabled => Settings?.AutoCheckOnStartup == true;
        public bool IsNotificationEnabled => Settings?.AutoCheckOnStartup == true;

        #endregion

        #region Constructor

        public UpdateViewModel(IUpdateService updateService, ICommonServices commonServices) : base(commonServices)
        {
            _updateService = updateService;
            _settings = new UpdateSettings();
        }

        #endregion

        #region Initialization

        public async Task InitializeAsync()
        {
            await LoadSettingsAsync();
            await CheckInitialStateAsync();
        }

        private async Task LoadSettingsAsync()
        {
            try
            {
                Settings = await _updateService.GetSettingsAsync();
                UpdateLastCheckText();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Settings yükleme hatası: {ex.Message}");
            }
        }

        private async Task CheckInitialStateAsync()
        {
            try
            {
                // Uygulama başlarken bekleyen güncelleme var mı kontrol et
                if (_updateService.IsUpdatePendingRestart)
                {
                    CurrentState = UpdateState.RestartRequired;
                    ProgressText = "Yeniden başlatma bekleniyor";
                    return;
                }

                // Auto-check enabled ise ve gerekli süre geçmişse kontrol yap
                if (Settings.AutoCheckOnStartup && ShouldAutoCheck())
                {
                    await CheckForUpdatesAsync();
                }
                else
                {
                    CurrentState = UpdateState.Idle;
                }
            }
            catch (Exception ex)
            {
                CurrentState = UpdateState.Error;
                ErrorMessage = ex.Message;
            }
        }

        private bool ShouldAutoCheck()
        {
            if (Settings.LastCheckDate == null) return true;

            var hoursSinceLastCheck = (DateTime.Now - Settings.LastCheckDate.Value).TotalHours;
            return hoursSinceLastCheck >= Settings.CheckIntervalHours;
        }

        #endregion

        #region Commands - Eski isimler korundu

        public ICommand CheckNowCommand => new RelayCommand(async () => await CheckNowAsync(), () => IsCheckButtonEnabled);
        public ICommand UpdateActionCommand => new RelayCommand(async () => await UpdateActionAsync());
        public ICommand OpenChangelogCommand => new RelayCommand(async () => await OpenChangelogAsync());

        // Settings Commands - eski isimler
        public ICommand ToggleAutoCheckCommand => new RelayCommand(async () => await SaveSettingsAsync());
        public ICommand ToggleAutoDownloadCommand => new RelayCommand(async () => await SaveSettingsAsync());
        public ICommand ToggleShowNotificationsCommand => new RelayCommand(async () => await SaveSettingsAsync());
        public ICommand ToggleIncludeBetaCommand => new RelayCommand(async () => await SaveSettingsAsync());

        #endregion

        #region Command Implementations

        private async Task CheckNowAsync()
        {
            await CheckForUpdatesAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                CurrentState = UpdateState.Checking;
                ProgressText = "Güncelleştirmeler kontrol ediliyor...";
                ErrorMessage = "";

                var updateInfo = await _updateService.CheckForUpdatesAsync(Settings.AllowPrereleaseVersions);

                if (updateInfo != null)
                {
                    _currentUpdateInfo = updateInfo;
                    CurrentState = UpdateState.UpdateAvailable;
                    ProgressText = "Güncelleme indirilebilir";
                }
                else
                {
                    CurrentState = UpdateState.Idle;
                    ProgressText = "";
                }

                UpdateLastCheckText();
            }
            catch (Exception ex)
            {
                CurrentState = UpdateState.Error;
                ErrorMessage = ex.Message;
                ProgressText = "Kontrol başarısız";
            }
        }

        private async Task UpdateActionAsync()
        {
            try
            {
                switch (CurrentState)
                {
                    case UpdateState.Idle:
                    case UpdateState.Error:
                        await CheckForUpdatesAsync();
                        break;

                    case UpdateState.UpdateAvailable:
                        await DownloadUpdateAsync();
                        break;

                    case UpdateState.Downloaded:
                        InstallUpdate();
                        break;
                }
            }
            catch (Exception ex)
            {
                CurrentState = UpdateState.Error;
                ErrorMessage = ex.Message;
            }
        }

        private async Task DownloadUpdateAsync()
        {
            try
            {
                if (_currentUpdateInfo == null) return;

                CurrentState = UpdateState.Downloading;
                ProgressText = "İndiriliyor...";
                ProgressPercentage = 0;

                var progress = new Progress<int>(percentage =>
                {
                    DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
                    {
                        ProgressPercentage = percentage;
                        ProgressDetails = $"{percentage}% tamamlandı";
                    });
                });

                await _updateService.DownloadUpdatesAsync(progress);

                CurrentState = UpdateState.Downloaded;
                ProgressText = "Güncelleme kurulmaya hazır";
                ProgressPercentage = 100;
            }
            catch (Exception ex)
            {
                CurrentState = UpdateState.Error;
                ErrorMessage = ex.Message;
                ProgressText = "İndirme başarısız";
            }
        }

        private void InstallUpdate()
        {
            try
            {
                if (_currentUpdateInfo == null) return;

                CurrentState = UpdateState.Installing;
                ProgressText = "Uygulama yeniden başlatılıyor...";

                _updateService.ApplyUpdatesAndRestart();
            }
            catch (Exception ex)
            {
                CurrentState = UpdateState.Error;
                ErrorMessage = ex.Message;
                ProgressText = "Kurulum başarısız";
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

        #region Settings - Eski method isimleri korundu

        public async Task UpdateCheckInterval(int hours)
        {
            if (Settings != null)
            {
                Settings.CheckIntervalHours = hours;
                await SaveSettingsAsync();
            }
        }

        private async Task SaveSettingsAsync()
        {
            try
            {
                await _updateService.SaveSettingsAsync(Settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Settings kaydetme hatası: {ex.Message}");
            }
        }

        #endregion

        #region UI Helper Methods - Eski method isimleri

        private string GetStatusText()
        {
            return CurrentState switch
            {
                UpdateState.Idle => "Güncelsiniz",
                UpdateState.Checking => "Güncelleştirmeler denetleniyor...",
                UpdateState.UpdateAvailable => "Bir güncelleştirme hazır",
                UpdateState.Downloading => "İndiriliyor...",
                UpdateState.Downloaded => "Yüklemeye hazır",
                UpdateState.Installing => "Yükleniyor...",
                UpdateState.RestartRequired => "Yeniden başlatma bekleniyor",
                UpdateState.Error => "Bir sorun oluştu",
                _ => "Güncelsiniz"
            };
        }

        private string GetVersionText()
        {
            return CurrentState switch
            {
                UpdateState.UpdateAvailable when _currentUpdateInfo != null => $"v{_currentUpdateInfo.TargetFullRelease.Version} hazır",
                UpdateState.Downloaded when _currentUpdateInfo != null => $"v{_currentUpdateInfo.TargetFullRelease.Version} indirildi",
                UpdateState.Error => "Uygulama güncellenemedi",
                _ => "Lütfen bekleyin..."
            };
        }

        private string GetButtonText()
        {
            return CurrentState switch
            {
                UpdateState.Idle => "Kontrol Et",
                UpdateState.Checking => "Kontrol Ediliyor...",
                UpdateState.UpdateAvailable => "İndir",
                UpdateState.Downloading => "İndiriliyor...",
                UpdateState.Downloaded => "Yükle ve Yeniden Başlat",
                UpdateState.Installing => "Yükleniyor...",
                UpdateState.RestartRequired => "Yeniden Başlatılıyor...",
                UpdateState.Error => "Tekrar Dene",
                _ => "Kontrol Et"
            };
        }

        private bool GetButtonEnabled()
        {
            return CurrentState switch
            {
                UpdateState.Checking => false,
                UpdateState.Downloading => false,
                UpdateState.Installing => false,
                UpdateState.RestartRequired => false,
                _ => true
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
                UpdateState.RestartRequired or
                UpdateState.Error => true,
                _ => false,
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

        private string GetStatusIcon()
        {
            return CurrentState switch
            {
                UpdateState.Idle or UpdateState.RestartRequired => "\uE930", // CheckmarkBold
                UpdateState.Checking or UpdateState.Downloading or UpdateState.Installing => "\uE895", // Sync
                UpdateState.UpdateAvailable or UpdateState.Downloaded => "\uE946", // Info
                UpdateState.Error => "\uE783", // Error
                _ => "\uE946",
            };
        }

        private Brush GetStatusIconBrush()
        {
            var brushName = CurrentState switch
            {
                UpdateState.Idle or UpdateState.RestartRequired => "SystemFillColorSuccessBrush",
                UpdateState.Error => "SystemFillColorCriticalBrush",
                _ => "SystemControlForegroundAccentBrush",
            };
            return (Brush)Application.Current.Resources[brushName];
        }

        private void UpdateUIProperties()
        {
            DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
            {
                // Tüm eski binding property'leri güncelle
                NotifyPropertyChanged(nameof(StatusText));
                NotifyPropertyChanged(nameof(VersionText));
                NotifyPropertyChanged(nameof(UpdateButtonText));
                NotifyPropertyChanged(nameof(IsUpdateButtonEnabled));
                NotifyPropertyChanged(nameof(IsCheckButtonEnabled));
                NotifyPropertyChanged(nameof(UpdateCardVisibility));
                NotifyPropertyChanged(nameof(UpdateDetailsVisibility));
                NotifyPropertyChanged(nameof(ProgressVisibility));
                NotifyPropertyChanged(nameof(ErrorVisibility));
                NotifyPropertyChanged(nameof(UpdateInfoVisibility));
                NotifyPropertyChanged(nameof(ChangelogButtonVisibility));
                NotifyPropertyChanged(nameof(StatusIconGlyph));
                NotifyPropertyChanged(nameof(StatusIconBrush));
                NotifyPropertyChanged(nameof(UpdateSize));
                NotifyPropertyChanged(nameof(ReleaseDate));
                NotifyPropertyChanged(nameof(ChangelogUrl));
            });
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

        #endregion

        #region Cleanup - Eski Unsubscribe method adı korundu

        public void Unsubscribe()
        {
            // Artık MessageService yok, bu method boş kalacak
            // Ama eski kod uyumluluğu için tutuldu
        }

        #endregion
    }
}
