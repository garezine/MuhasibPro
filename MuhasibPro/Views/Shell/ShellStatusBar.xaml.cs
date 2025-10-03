using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using MuhasibPro.Helpers;
using System.ComponentModel;

namespace MuhasibPro.Views.Shell
{
    public sealed partial class ShellStatusBar : UserControl, INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private readonly StatusBarHelpers _statusManager;

        public ShellStatusBar()
        {
            InitializeComponent();
            _statusManager = StatusBarHelpers.Instance;

            // StatusBarManager'dan PropertyChanged olaylarını dinle
            _statusManager.PropertyChanged += OnStatusManagerPropertyChanged;

            // Timer'ı başlat
            InitializeTimer();
        }

        #region Timer

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) =>
            {
                TimeDisplay.Text = DateTime.Now.ToString("HH:mm:ss");
            };
            _timer.Start();
        }

        #endregion

        #region Property Changed Handler

        private void OnStatusManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                switch (e.PropertyName)
                {
                    case nameof(StatusBarHelpers.StatusMessage):
                        StatusMessage = _statusManager.StatusMessage;
                        break;

                    case nameof(StatusBarHelpers.MessageType):
                        UpdateStatusAppearance();
                        break;

                    case nameof(StatusBarHelpers.UserName):
                        UserName = _statusManager.UserName;
                        OnPropertyChanged(nameof(ShowUserInfo));
                        break;

                    case nameof(StatusBarHelpers.DatabaseConnectionMessage):
                        DatabaseConnectionMessage = _statusManager.DatabaseConnectionMessage;
                        OnPropertyChanged(nameof(ShowDatabaseInfo));
                        break;

                    case nameof(StatusBarHelpers.IsDatabaseConnection):
                        IsDatabaseConnection = _statusManager.IsDatabaseConnection;
                        OnPropertyChanged(nameof(DatabaseIconBrush));
                        break;

                    case nameof(StatusBarHelpers.IsSaveStatus):
                        IsSaveStatus = _statusManager.IsSaveStatus;
                        break;

                    case nameof(StatusBarHelpers.IsStatusProgress):
                        IsStatusProgress = _statusManager.IsStatusProgress;
                        OnPropertyChanged(nameof(ShowProgressBar));
                        break;

                    case nameof(StatusBarHelpers.IsProgressIndeterminate):
                        IsProgressIndeterminate = _statusManager.IsProgressIndeterminate;
                        OnPropertyChanged(nameof(ShowProgressPercent));
                        break;

                    case nameof(Helpers.StatusBarHelpers.ProgressValue):
                        ProgressValue = _statusManager.ProgressValue;
                        OnPropertyChanged(nameof(ProgressText));
                        break;
                }
            });
        }

        #endregion

        #region Properties

        private string _statusMessage = "Hazır";
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        private string _userName;
        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        private string _databaseConnectionMessage;
        public string DatabaseConnectionMessage
        {
            get => _databaseConnectionMessage;
            set
            {
                _databaseConnectionMessage = value;
                OnPropertyChanged(nameof(DatabaseConnectionMessage));
            }
        }

        private bool _isSaveStatus;
        public bool IsSaveStatus
        {
            get => _isSaveStatus;
            set
            {
                _isSaveStatus = value;
                OnPropertyChanged(nameof(IsSaveStatus));
            }
        }

        private bool _isStatusProgress;
        public bool IsStatusProgress
        {
            get => _isStatusProgress;
            set
            {
                _isStatusProgress = value;
                OnPropertyChanged(nameof(IsStatusProgress));
            }
        }

        private bool _isDatabaseConnection;
        public bool IsDatabaseConnection
        {
            get => _isDatabaseConnection;
            set
            {
                _isDatabaseConnection = value;
                OnPropertyChanged(nameof(IsDatabaseConnection));
            }
        }

        private bool _isProgressIndeterminate = true;
        public bool IsProgressIndeterminate
        {
            get => _isProgressIndeterminate;
            set
            {
                _isProgressIndeterminate = value;
                OnPropertyChanged(nameof(IsProgressIndeterminate));
            }
        }

        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value;
                OnPropertyChanged(nameof(ProgressValue));
            }
        }

        #endregion

        #region Computed Properties

        // Status rengi - mesaj tipine göre
        private SolidColorBrush _statusBrush = new SolidColorBrush(Colors.DeepSkyBlue);
        public SolidColorBrush StatusBrush
        {
            get => _statusBrush;
            set
            {
                _statusBrush = value;
                OnPropertyChanged(nameof(StatusBrush));
            }
        }

        // Status ikonu - mesaj tipine göre
        private string _statusGlyph = "\uE946";
        public string StatusGlyph
        {
            get => _statusGlyph;
            set
            {
                _statusGlyph = value;
                OnPropertyChanged(nameof(StatusGlyph));
            }
        }

        // İkon görünürlüğü
        public Visibility ShowStatusIcon =>
            _statusManager.MessageType != StatusMessageType.Info
                ? Visibility.Visible
                : Visibility.Collapsed;

        // Progress bar görünürlüğü
        public Visibility ShowProgressBar =>
            IsStatusProgress && !IsProgressIndeterminate
                ? Visibility.Visible
                : Visibility.Collapsed;

        // Progress yüzde görünürlüğü
        public Visibility ShowProgressPercent =>
            IsStatusProgress && !IsProgressIndeterminate
                ? Visibility.Visible
                : Visibility.Collapsed;

        // Progress text
        public string ProgressText => $"{ProgressValue:F0}%";

        // Kullanıcı bilgisi görünürlüğü
        public Visibility ShowUserInfo =>
            !string.IsNullOrEmpty(UserName)
                ? Visibility.Visible
                : Visibility.Collapsed;

        // Database bilgisi görünürlüğü
        public Visibility ShowDatabaseInfo =>
            !string.IsNullOrEmpty(DatabaseConnectionMessage)
                ? Visibility.Visible
                : Visibility.Collapsed;

        // Database ikon rengi
        public SolidColorBrush DatabaseIconBrush =>
            IsDatabaseConnection
                ? new SolidColorBrush(Colors.LimeGreen)
                : new SolidColorBrush(Colors.OrangeRed);

        #endregion

        #region Methods

        private void UpdateStatusAppearance()
        {
            var messageType = _statusManager.MessageType;

            // Renk güncelle
            StatusBrush = new SolidColorBrush(messageType switch
            {
                StatusMessageType.Success => Colors.LimeGreen,
                StatusMessageType.Warning => Colors.Orange,
                StatusMessageType.Error => Colors.OrangeRed,
                _ => Colors.DeepSkyBlue
            });

            // İkon güncelle
            StatusGlyph = messageType switch
            {
                StatusMessageType.Success => "\uE73E", // CheckMark
                StatusMessageType.Warning => "\uE7BA", // Warning
                StatusMessageType.Error => "\uE783",   // ErrorBadge
                _ => "\uE946"                          // Info
            };

            OnPropertyChanged(nameof(ShowStatusIcon));
        }

        #endregion

        #region Cleanup

        ~ShellStatusBar()
        {
            if (_statusManager != null)
            {
                _statusManager.PropertyChanged -= OnStatusManagerPropertyChanged;
            }

            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}