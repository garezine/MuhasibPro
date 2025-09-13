using Microsoft.UI.Dispatching;
using System.ComponentModel;

namespace MuhasibPro.ViewModels.Manager
{
    public class StatusBarManager : INotifyPropertyChanged
    {
        private static StatusBarManager _instance;
        private static readonly object _lock = new object();


        private string _statusMessage = "Hazır";
        private string _userName;
        private string _databaseConnectionMessage;
        private bool _isError;
        private bool _isSaveStatus;
        private bool _isStatusProgress;
        private bool _isDatabaseConnection;

        public static StatusBarManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new StatusBarManager();
                    }
                }
                return _instance;
            }
        }

        private static DispatcherQueue dispatcherQueue;

        // DispatcherQueue'yu başlatmak için bir metod
        public static void Initialize(DispatcherQueue queue)
        {
            dispatcherQueue = queue ?? throw new ArgumentNullException(nameof(queue));
        }

        private StatusBarManager()
        {
            // Fallback olarak mevcut thread'in dispatcher'ını al
            dispatcherQueue ??= DispatcherQueue.GetForCurrentThread();
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged(nameof(StatusMessage));
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

        public bool IsError
        {
            get => _isError;
            set
            {
                if (_isError != value)
                {
                    _isError = value;
                    OnPropertyChanged(nameof(IsError));
                    OnPropertyChanged(nameof(StatusFontIcon));
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

        public bool StatusFontIcon => IsError;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Helper methods for easy status updates
        public void ShowStatusMessage(string message, bool isError = false)
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                StatusMessage = message;
                IsError = isError;
            });
        }

        public void ShowProgress(string message = null)
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                if (!string.IsNullOrEmpty(message))
                    StatusMessage = message;
                IsStatusProgress = true;
            });
        }

        public void HideProgress()
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                IsStatusProgress = false;
            });
        }

        public void SetSaveStatus(bool isSaving)
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                IsSaveStatus = isSaving;
            });
        }

        public void SetDatabaseStatus(bool isConnected, string message = null)
        {
            if (dispatcherQueue != null)
            {
                dispatcherQueue.TryEnqueue(() =>
                {
                    IsDatabaseConnection = isConnected;
                    if (!string.IsNullOrEmpty(message))
                        DatabaseConnectionMessage = message;
                });
            }
            else
            {
                // Fallback: Doğrudan atama yapın
                IsDatabaseConnection = isConnected;
                if (!string.IsNullOrEmpty(message))
                    DatabaseConnectionMessage = message;
            }
        }
    }
}
