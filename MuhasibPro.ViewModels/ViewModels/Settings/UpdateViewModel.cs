using Muhasib.Business.Services.Contracts.CommonServices;
using Muhasib.Data.Managers.UpdataManager;
using Muhasib.Domain.Enum;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Windows.Input;


namespace MuhasibPro.ViewModels.ViewModels.Settings
{
    public partial class UpdateViewModel : ViewModelBase
    {
        private readonly IUpdateService _updateService;
        private UpdateSettingsModel _settings;
        private Velopack.UpdateInfo? _currentUpdateInfo;

        #region Constructor

        public UpdateViewModel(IUpdateService updateService, ICommonServices commonServices) : base(commonServices)
        {
            _updateService = updateService;
            _settings = new UpdateSettingsModel();
        }

        #endregion

        #region Properties

        private UpdateState _currentState = UpdateState.Idle;
        public UpdateState CurrentState
        {
            get => _currentState;
            set
            {
                if (Set(ref _currentState, value))
                {
                    _ = Task.Run(async () => await UpdateUIProperties());
                }
            }
        }


        private int _progressPercentage;
        public int ProgressPercentage
        {
            get => _progressPercentage;
            set => Set(ref _progressPercentage, value);
        }

        private string _progressText = string.Empty;
        public string ProgressText
        {
            get => _progressText;
            set => Set(ref _progressText, value);
        }

        private string _progressDetails = string.Empty;
        public string ProgressDetails
        {
            get => _progressDetails;
            set => Set(ref _progressDetails, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                Set(ref _errorMessage, value);

            }
        }

        private string _lastCheckText = "Son denetleme: Hiçbir zaman";
        public string LastCheckText
        {
            get => _lastCheckText;
            set => Set(ref _lastCheckText, value);
        }

        // YENİ PROPERTY'LER
        public bool AutoCheckEnabled
        {
            get => Settings?.AutoCheckOnStartup == true;
            set
            {
                if (Settings != null && Settings.AutoCheckOnStartup != value)
                {
                    Settings.AutoCheckOnStartup = value;
                    NotifyPropertyChanged(nameof(AutoCheckEnabled));
                    _ = SaveSettingsAsync(); // Otomatik kaydet
                }
            }
        }

        public bool NotificationsEnabled
        {
            get => Settings?.ShowNotifications == true;
            set
            {
                if (Settings != null && Settings.ShowNotifications != value)
                {
                    Settings.ShowNotifications = value;
                    NotifyPropertyChanged(nameof(NotificationsEnabled));
                    _ = SaveSettingsAsync(); // Otomatik kaydet
                }
            }
        }

        public bool IncludeBetaVersionsEnabled
        {
            get => Settings?.IncludeBetaVersions == true;
            set
            {
                if (Settings != null && Settings.IncludeBetaVersions != value)
                {
                    Settings.IncludeBetaVersions = value;
                    NotifyPropertyChanged(nameof(IncludeBetaVersionsEnabled));
                    _ = SaveSettingsAsync(); // Otomatik kaydet
                }
            }
        }
        public UpdateSettingsModel Settings
        {
            get => _settings;
            set
            {
                if (Set(ref _settings, value))
                {
                    // Settings değiştiğinde tüm ilgili property'leri güncelle
                    NotifyPropertyChanged(nameof(AutoCheckEnabled));
                    NotifyPropertyChanged(nameof(NotificationsEnabled));
                    NotifyPropertyChanged(nameof(IncludeBetaVersionsEnabled));
                }
            }
        }

        // UI States - eski XAML bindingler
        public string StatusText => GetStatusText(); // güncellendi.
        public string VersionText => GetVersionText();
        public string UpdateButtonText => GetButtonText();
        public bool IsUpdateButtonEnabled => GetButtonEnabled();
        public bool IsCheckButtonEnabled => CurrentState != UpdateState.Checking; // güncellendi.

        // Visibilities - eski XAML bindingler





        // Icon and Colors - eski XAML bindingler
        public string StatusIconGlyph => GetStatusIcon();// güncellendi.


        // Update Info - eski XAML binding isimleri korundu
        public string UpdateSize => _currentUpdateInfo?.TargetFullRelease.Size.ToString("N0") + " bytes" ?? string.Empty;
        public string ReleaseDate => _currentUpdateInfo?.TargetFullRelease.Version.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("tr-TR")) ?? string.Empty;

        // Eski XAML'deki ChangelogUrl binding
        public string ChangelogUrl => _currentUpdateInfo != null ?
            $"https://github.com/garezine/MuhasibPro/releases/tag/v{_currentUpdateInfo.TargetFullRelease.Version}" : string.Empty;

        #endregion



        #region Initialization

        public async Task InitializeAsync()
        {
            await LoadSettingsAsync(); // Settings yüklendi

            // ÖNCE UI'ı güncelle
            await ContextService.RunAsync(() =>
            {
                NotifyPropertyChanged(nameof(AutoCheckEnabled));
                NotifyPropertyChanged(nameof(NotificationsEnabled));
                NotifyPropertyChanged(nameof(IncludeBetaVersionsEnabled));
            });

            // SONRA state kontrolü yap
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
                if (AutoCheckEnabled)
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



        #endregion

        #region Commands

        public ICommand CheckNowCommand => new AsyncRelayCommand(CheckNowAsync, () => IsCheckButtonEnabled);
        public ICommand UpdateActionCommand => new AsyncRelayCommand(UpdateActionAsync);




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
                ErrorMessage = string.Empty;

                var updateInfo = await _updateService.CheckForUpdatesAsync(IncludeBetaVersionsEnabled);

                if (updateInfo != null)
                {
                    _currentUpdateInfo = updateInfo;
                    CurrentState = UpdateState.UpdateAvailable;
                    ProgressText = "Güncelleme indirilebilir";
                }
                else
                {
                    CurrentState = UpdateState.Idle;
                    ProgressText = string.Empty;
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

                var progress = new Progress<int>(async percentage =>
                {
                    await ContextService.RunAsync(() =>
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



        #endregion

        #region Settings


        private async Task SaveSettingsAsync()
        {
            try
            {
                // DEBUG: Hangi ayarın değiştiğini görelim
                System.Diagnostics.Debug.WriteLine($"Settings saving - AutoCheck: {AutoCheckEnabled}, Notifications: {NotificationsEnabled}, Beta: {IncludeBetaVersionsEnabled}");

                await _updateService.SaveSettingsAsync(Settings);

                System.Diagnostics.Debug.WriteLine("Settings saved successfully");
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
        } // güncellendi

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

        public bool ShouldShowUpdateCard()
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

        public bool ShouldShowDetails()
        {
            return CurrentState == UpdateState.UpdateAvailable ||
                   CurrentState == UpdateState.Downloaded ||
                   CurrentState == UpdateState.Downloading;
        }

        public bool IsProgressVisible()
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



        private async Task UpdateUIProperties()
        {
            await ContextService.RunAsync(() =>
            {
                // Tüm eski binding property'leri güncelle
                NotifyPropertyChanged(nameof(StatusText));
                NotifyPropertyChanged(nameof(VersionText));
                NotifyPropertyChanged(nameof(UpdateButtonText));
                NotifyPropertyChanged(nameof(IsUpdateButtonEnabled));
                NotifyPropertyChanged(nameof(IsCheckButtonEnabled));
                NotifyPropertyChanged(nameof(StatusIconGlyph));
                NotifyPropertyChanged(nameof(UpdateSize));
                NotifyPropertyChanged(nameof(ReleaseDate));
                NotifyPropertyChanged(nameof(ChangelogUrl));
            });
        }

        private void UpdateLastCheckText()
        {
            if (Settings?.LastCheckTime != null)
            {
                var timeAgo = DateTime.Now - Settings.LastCheckTime.Value;
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
                    timeText = Settings.LastCheckTime.Value.ToString("dd.MM.yyyy");

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
