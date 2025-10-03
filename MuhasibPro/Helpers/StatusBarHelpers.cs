using Microsoft.UI;
using Microsoft.UI.Dispatching;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;

namespace MuhasibPro.Helpers
{
    // Mesaj tipleri enum
    public enum StatusMessageType
    {
        Info,       // Normal bilgi - Mavi
        Success,    // Başarı - Yeşil
        Warning,    // Uyarı - Turuncu
        Error       // Hata - Kırmızı
    }

    public class StatusBarHelpers : INotifyPropertyChanged
    {
        private static StatusBarHelpers _instance;
        private static readonly object _lock = new object();
        private static DispatcherQueue dispatcherQueue;
        private CancellationTokenSource _autoHideCts;

        #region Singleton

        public static StatusBarHelpers Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new StatusBarHelpers();
                    }
                }
                return _instance;
            }
        }

        public static void Initialize(DispatcherQueue queue)
        {
            dispatcherQueue = queue ?? throw new ArgumentNullException(nameof(queue));
        }

        private StatusBarHelpers()
        {
            dispatcherQueue ??= DispatcherQueue.GetForCurrentThread();
        }

        #endregion

        #region Private Fields

        private string _statusMessage = "Hazır";
        private string _userName;
        private string _databaseConnectionMessage;
        private StatusMessageType _messageType = StatusMessageType.Info;
        private bool _isSaveStatus;
        private bool _isStatusProgress;
        private bool _isDatabaseConnection;
        private bool _isProgressIndeterminate = true;
        private double _progressValue;
        private DateTime _lastUpdateTime = DateTime.Now;

        #endregion

        #region Public Properties

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    _lastUpdateTime = DateTime.Now;
                    OnPropertyChanged(nameof(StatusMessage));
                    OnPropertyChanged(nameof(LastUpdateTime));
                }
            }
        }

        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        public string DatabaseConnectionMessage
        {
            get => _databaseConnectionMessage;
            set
            {
                if (_databaseConnectionMessage != value)
                {
                    _databaseConnectionMessage = value;
                    OnPropertyChanged(nameof(DatabaseConnectionMessage));
                }
            }
        }

        public StatusMessageType MessageType
        {
            get => _messageType;
            set
            {
                if (_messageType != value)
                {
                    _messageType = value;
                    OnPropertyChanged(nameof(MessageType));
                    OnPropertyChanged(nameof(StatusIconGlyph));
                    OnPropertyChanged(nameof(StatusColor));
                }
            }
        }

        public bool IsSaveStatus
        {
            get => _isSaveStatus;
            set
            {
                if (_isSaveStatus != value)
                {
                    _isSaveStatus = value;
                    OnPropertyChanged(nameof(IsSaveStatus));
                }
            }
        }

        public bool IsStatusProgress
        {
            get => _isStatusProgress;
            set
            {
                if (_isStatusProgress != value)
                {
                    _isStatusProgress = value;
                    OnPropertyChanged(nameof(IsStatusProgress));
                }
            }
        }

        public bool IsDatabaseConnection
        {
            get => _isDatabaseConnection;
            set
            {
                if (_isDatabaseConnection != value)
                {
                    _isDatabaseConnection = value;
                    OnPropertyChanged(nameof(IsDatabaseConnection));
                }
            }
        }

        public bool IsProgressIndeterminate
        {
            get => _isProgressIndeterminate;
            set
            {
                if (_isProgressIndeterminate != value)
                {
                    _isProgressIndeterminate = value;
                    OnPropertyChanged(nameof(IsProgressIndeterminate));
                }
            }
        }

        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                if (Math.Abs(_progressValue - value) > 0.01)
                {
                    _progressValue = Math.Clamp(value, 0, 100);
                    OnPropertyChanged(nameof(ProgressValue));
                    OnPropertyChanged(nameof(ProgressText));
                }
            }
        }

        public DateTime LastUpdateTime
        {
            get => _lastUpdateTime;
            private set
            {
                _lastUpdateTime = value;
                OnPropertyChanged(nameof(LastUpdateTime));
                OnPropertyChanged(nameof(LastUpdateTimeText));
            }
        }

        #endregion

        #region Computed Properties

        // İkon - mesaj tipine göre otomatik değişir
        public string StatusIconGlyph => MessageType switch
        {
            StatusMessageType.Success => "\uE73E", // CheckMark
            StatusMessageType.Warning => "\uE7BA", // Warning
            StatusMessageType.Error => "\uE783",   // ErrorBadge
            _ => "\uE946"                          // Info
        };

        // Renk - mesaj tipine göre
        public Color StatusColor => MessageType switch
        {
            StatusMessageType.Success => Colors.LimeGreen,
            StatusMessageType.Warning => Colors.Orange,
            StatusMessageType.Error => Colors.OrangeRed,
            _ => Colors.DeepSkyBlue
        };

        // Progress text - yüzdelik gösterim
        public string ProgressText => IsProgressIndeterminate
            ? ""
            : $"{ProgressValue:F0}%";

        // Son güncelleme zamanı text
        public string LastUpdateTimeText =>
            $"Son güncelleme: {LastUpdateTime:HH:mm:ss}";

        // İkon görünürlüğü
        public bool ShowStatusIcon => MessageType != StatusMessageType.Info;

        #endregion

        #region Helper Methods

        /// <summary>
        /// Basit mesaj göster (otomatik kapanma YOK)
        /// </summary>
        public void ShowMessage(string message, StatusMessageType type = StatusMessageType.Info)
        {
            ExecuteOnUIThread(() =>
            {
                _autoHideCts?.Cancel();
                StatusMessage = message;
                MessageType = type;
                IsStatusProgress = false;
            });
        }

        /// <summary>
        /// Otomatik kapanan mesaj göster
        /// </summary>
        public void ShowMessage(string message, StatusMessageType type, int autoHideSeconds)
        {
            ShowMessage(message, type);

            if (autoHideSeconds > 0)
            {
                _autoHideCts = new CancellationTokenSource();
                _ = AutoHideAfterDelay(TimeSpan.FromSeconds(autoHideSeconds), _autoHideCts.Token);
            }
        }

        /// <summary>
        /// Başarı mesajı (3 saniye sonra otomatik kapanır)
        /// </summary>
        public void ShowSuccess(string message, int autoHideSeconds = 3)
        {
            ShowMessage(message, StatusMessageType.Success, autoHideSeconds);
        }

        /// <summary>
        /// Uyarı mesajı (4 saniye sonra otomatik kapanır)
        /// </summary>
        public void ShowWarning(string message, int autoHideSeconds = 4)
        {
            ShowMessage(message, StatusMessageType.Warning, autoHideSeconds);
        }

        /// <summary>
        /// Hata mesajı (5 saniye sonra otomatik kapanır)
        /// </summary>
        public void ShowError(string message, int autoHideSeconds = 5)
        {
            ShowMessage(message, StatusMessageType.Error, autoHideSeconds);
        }

        /// <summary>
        /// Bilgi mesajı (otomatik kapanmaz)
        /// </summary>
        public void ShowInfo(string message)
        {
            ShowMessage(message, StatusMessageType.Info);
        }

        /// <summary>
        /// Progress göster (belirsiz süre)
        /// </summary>
        public void ShowProgress(string message = null)
        {
            ExecuteOnUIThread(() =>
            {
                _autoHideCts?.Cancel();
                if (!string.IsNullOrEmpty(message))
                    StatusMessage = message;
                MessageType = StatusMessageType.Info;
                IsStatusProgress = true;
                IsProgressIndeterminate = true;
            });
        }

        /// <summary>
        /// Progress göster (yüzdelik)
        /// </summary>
        public void ShowProgress(string message, double progressPercent)
        {
            ExecuteOnUIThread(() =>
            {
                _autoHideCts?.Cancel();
                if (!string.IsNullOrEmpty(message))
                    StatusMessage = message;
                MessageType = StatusMessageType.Info;
                IsStatusProgress = true;
                IsProgressIndeterminate = false;
                ProgressValue = progressPercent;
            });
        }

        /// <summary>
        /// Progress güncelle
        /// </summary>
        public void UpdateProgress(double progressPercent)
        {
            ExecuteOnUIThread(() =>
            {
                ProgressValue = progressPercent;
            });
        }

        /// <summary>
        /// Progress'i gizle
        /// </summary>
        public void HideProgress()
        {
            ExecuteOnUIThread(() =>
            {
                IsStatusProgress = false;
                IsProgressIndeterminate = true;
                ProgressValue = 0;
            });
        }

        /// <summary>
        /// StatusBar'ı temizle - "Hazır" durumuna döndür
        /// </summary>
        public void Clear()
        {
            ExecuteOnUIThread(() =>
            {
                _autoHideCts?.Cancel();
                StatusMessage = "Hazır";
                MessageType = StatusMessageType.Info;
                IsStatusProgress = false;
                IsProgressIndeterminate = true;
                ProgressValue = 0;
            });
        }

        /// <summary>
        /// Kayıt durumunu ayarla
        /// </summary>
        public void SetSaveStatus(bool isSaving)
        {
            ExecuteOnUIThread(() =>
            {
                IsSaveStatus = isSaving;
            });
        }

        /// <summary>
        /// Veritabanı bağlantı durumunu ayarla
        /// </summary>
        public void SetDatabaseStatus(bool isConnected, string message = null)
        {
            ExecuteOnUIThread(() =>
            {
                IsDatabaseConnection = isConnected;
                if (!string.IsNullOrEmpty(message))
                    DatabaseConnectionMessage = message;
            });
        }

        #endregion

        #region Legacy Support (Geriye uyumluluk)

        // Eski kodlarla uyumluluk için
        [Obsolete("Use ShowError() instead")]
        public void ShowStatusMessage(string message, bool isError = false)
        {
            if (isError)
                ShowError(message);
            else
                ShowInfo(message);
        }

        // Eski IsError property'si için
        public bool IsError
        {
            get => MessageType == StatusMessageType.Error;
            set
            {
                if (value && MessageType != StatusMessageType.Error)
                    MessageType = StatusMessageType.Error;
                else if (!value && MessageType == StatusMessageType.Error)
                    MessageType = StatusMessageType.Info;
            }
        }

        #endregion

        #region Private Methods

        private void ExecuteOnUIThread(Action action)
        {
            if (dispatcherQueue != null)
            {
                dispatcherQueue.TryEnqueue(() => action());
            }
            else
            {
                action();
            }
        }

        private async Task AutoHideAfterDelay(TimeSpan delay, CancellationToken ct)
        {
            try
            {
                await Task.Delay(delay, ct);
                if (!ct.IsCancellationRequested)
                {
                    Clear();
                }
            }
            catch (TaskCanceledException)
            {
                // Normal iptal durumu
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}