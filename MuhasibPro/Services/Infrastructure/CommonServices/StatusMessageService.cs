using Microsoft.UI.Dispatching;
using Muhasib.Domain.Enum;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using System.ComponentModel;
using System.Diagnostics;

namespace MuhasibPro.Services.Infrastructure.CommonServices
{
    public class StatusMessageService : IStatusMessageService
    {
        private DispatcherQueue _dispatcherQueue;
        private CancellationTokenSource _autoHideCts;

        private string _statusMessage = "Hazır";
        private StatusMessageType _messageType = StatusMessageType.Info;
        private bool _isProgressVisible;
        private bool _isProgressIndeterminate = true;
        private double _progressValue;
        private DateTime _lastUpdateTime = DateTime.Now;

        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties
        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                if(_statusMessage != value)
                {
                    _statusMessage = value;
                    _lastUpdateTime = DateTime.Now;
                    NotifyPropertyChanged(nameof(StatusMessage));
                    NotifyPropertyChanged(nameof(LastUpdateTime));
                    NotifyPropertyChanged(nameof(LastUpdateTimeText));
                }
            }
        }

        public bool ShowProgressBar => IsProgressVisible && !IsProgressIndeterminate;

        public StatusMessageType MessageType
        {
            get => _messageType;
            private set
            {
                if(_messageType != value)
                {
                    _messageType = value;
                    NotifyPropertyChanged(nameof(MessageType));
                    NotifyPropertyChanged(nameof(StatusIconGlyph));
                    NotifyPropertyChanged(nameof(StatusColorHex));
                    NotifyPropertyChanged(nameof(ShowStatusIcon));
                }
            }
        }

        private void OnProgressPropertiesChanged()
        {
            NotifyPropertyChanged(nameof(ShowProgressBar));
            NotifyPropertyChanged(nameof(ProgressText));
        }

        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            private set
            {
                if(_isProgressVisible != value)
                {
                    _isProgressVisible = value;
                    NotifyPropertyChanged(nameof(IsProgressVisible));
                }
            }
        }

        public bool IsProgressIndeterminate
        {
            get => _isProgressIndeterminate;
            private set
            {
                if(_isProgressIndeterminate != value)
                {
                    _isProgressIndeterminate = value;
                    NotifyPropertyChanged(nameof(IsProgressIndeterminate));
                }
            }
        }

        public double ProgressValue
        {
            get => _progressValue;
            private set
            {
                if(Math.Abs(_progressValue - value) > 0.01)
                {
                    _progressValue = Math.Clamp(value, 0, 100);
                    NotifyPropertyChanged(nameof(ProgressValue));
                    NotifyPropertyChanged(nameof(ProgressText));
                }
            }
        }

        public DateTime LastUpdateTime
        {
            get => _lastUpdateTime;
            private set
            {
                _lastUpdateTime = value;
                NotifyPropertyChanged(nameof(LastUpdateTime));
                NotifyPropertyChanged(nameof(LastUpdateTimeText));
            }
        }

        public string StatusIconGlyph => MessageType switch
        {
            StatusMessageType.Success => "\uE73E",
            StatusMessageType.Warning => "\uE7BA",
            StatusMessageType.Error => "\uE783",
            StatusMessageType.Edit => "\uE70F",          // ✏ - Edit
            StatusMessageType.Saving => "\uE74E",        // 💾 - Save  
            StatusMessageType.Deleting => "\uE74D",      // 🗑 - Delete
            StatusMessageType.MultipleSelect => "\uE73A",// ☑ - Checkbox
            StatusMessageType.Refreshing => "\uE72C",    // 🔄 - Refresh - YENİ
            _ => "\uE946"
        };

        public string StatusColorHex => MessageType switch
        {
            StatusMessageType.Success => "#FF32CD32",
            StatusMessageType.Warning => "#FFFFA500",
            StatusMessageType.Error => "#FFFF4500",
            StatusMessageType.Deleting => "#FFFFA500",   // Turuncu - Delete
            _ => "#FF00BFFF"
        };

        public string ProgressText => IsProgressIndeterminate ? string.Empty : $"{ProgressValue:F0}%";

        public string LastUpdateTimeText => $"Son güncelleme: {LastUpdateTime:HH:mm:ss}";

        public bool ShowStatusIcon => MessageType != StatusMessageType.Info;
        #endregion

        #region Initialization
        public void Initialize(object dispatcher) { _dispatcherQueue = dispatcher as DispatcherQueue; }
        #endregion

        #region Basic Methods
        public void ShowMessage(string message, StatusMessageType type, int autoHideSeconds)
        {
            ExecuteOnUIThread(() =>
            {
                CancelAutoHide();
                StatusMessage = SanitizeMessage(message);
                MessageType = type;
                IsProgressVisible = false;

                // ✅ SADECE autoHideSeconds > 0 ise setup et
                if (autoHideSeconds > 0)
                {
                    SetupAutoHide(autoHideSeconds);
                }
            });
        }

        public void Clear()
        {
            ExecuteOnUIThread(
                () =>
                {
                    CancelAutoHide();
                    StatusMessage = "Hazır";
                    MessageType = StatusMessageType.Info;
                    IsProgressVisible = false;
                    IsProgressIndeterminate = true;
                    ProgressValue = 0;
                });
        }
        #endregion

        #region Progress Methods
        

        public void ShowProgress(string message, double progressPercent = -1)
        {
            ExecuteOnUIThread(() =>
            {
                CancelAutoHide();
                if (!string.IsNullOrEmpty(message))
                    StatusMessage = SanitizeMessage(message);
                MessageType = StatusMessageType.Info;
                IsProgressVisible = true;

                // ✅ AKILLI PROGRESS DETECTION
                if (progressPercent < 0)
                {
                    IsProgressIndeterminate = true;  // Belirsiz progress
                    ProgressValue = 0;
                }
                else
                {
                    IsProgressIndeterminate = false; // Belirli progress
                    ProgressValue = Math.Clamp(progressPercent, 0, 100);
                }

                OnProgressPropertiesChanged();
            });
        }

        public void UpdateProgress(double progressPercent)
        {
            ExecuteOnUIThread(() => ProgressValue = progressPercent);
            OnProgressPropertiesChanged();
        }

        public void HideProgress()
        {
            ExecuteOnUIThread(
                () =>
                {
                    IsProgressVisible = false;
                    IsProgressIndeterminate = true;
                    ProgressValue = 0;
                    OnProgressPropertiesChanged();
                });
        }
        #endregion

        #region Advanced Async Methods
        public async Task ExecuteWithProgressAsync(
            Func<Task> action,
            string progressMessage,
            string successMessage = null,
            string errorMessage = null,
            bool measureTime = true,
            int successAutoHideSeconds = 3)
        {
            var stopwatch = measureTime ? Stopwatch.StartNew() : null;

            ShowProgress(progressMessage);

            try
            {
                await action();

                stopwatch?.Stop();

                if(!string.IsNullOrEmpty(successMessage))
                {
                    var message = measureTime && stopwatch != null
                        ? $"{successMessage} ({stopwatch.Elapsed.TotalSeconds:F3} saniye)"
                        : successMessage;

                    ExecuteOnUIThread(() => ShowMessage(message, StatusMessageType.Success, successAutoHideSeconds));
                    await Task.Delay(300);
                } else
                {
                    HideProgress();
                }
            } catch(Exception ex)
            {
                stopwatch?.Stop();

                var message = !string.IsNullOrEmpty(errorMessage) ? errorMessage : $"Hata: {ex.Message}";

                ExecuteOnUIThread(() => ShowMessage(message, StatusMessageType.Error,autoHideSeconds:-1));
                throw;
            }
        }

        public async Task ExecuteWithProgressAsync(
            Func<IProgress<double>, Task> action,
            string progressMessage,
            string successMessage = null,
            string errorMessage = null,
            bool measureTime = true,
            int successAutoHideSeconds = 3)
        {
            var stopwatch = measureTime ? Stopwatch.StartNew() : null;

            ShowProgress(progressMessage, 0);

            var progress = new Progress<double>(percent => UpdateProgress(percent));

            try
            {
                await action(progress);

                stopwatch?.Stop();

                if(!string.IsNullOrEmpty(successMessage))
                {
                    var message = measureTime && stopwatch != null
                        ? $"{successMessage} ({stopwatch.Elapsed.TotalSeconds:F3} saniye)"
                        : successMessage;
                    ExecuteOnUIThread(() => ShowMessage(message, StatusMessageType.Success, successAutoHideSeconds));
                    await Task.Delay(300);
                } else
                {
                    HideProgress();
                }
            } catch(Exception ex)
            {
                stopwatch?.Stop();

                var message = !string.IsNullOrEmpty(errorMessage) ? errorMessage : $"Hata: {ex.Message}";

                ExecuteOnUIThread(() => ShowMessage(message, StatusMessageType.Error, autoHideSeconds: -1));
                throw;
            }
        }

        public async Task ExecuteActionAsync(
            Func<Task> action,
            string startMessage = null,
            StatusMessageType startMessageType = StatusMessageType.Info,
            string successMessage = null,
            string errorMessage = null,
            bool measureTime = true,
            int successAutoHideSeconds = 3)
        {
            var stopwatch = measureTime ? Stopwatch.StartNew() : null;

            // Start message göster (progress'i temizle)
            if(!string.IsNullOrEmpty(startMessage))
            {
                ExecuteOnUIThread(
                    () =>
                    {
                        CancelAutoHide();
                        StatusMessage = SanitizeMessage(startMessage);
                        MessageType = startMessageType;
                        IsProgressVisible = false; // ✅ Progress'i temizle
                    });
            }
            await Task.Delay(300); // UI thread'in güncelleme yapması için kısa bir gecikme
            try
            {
                await action();

                stopwatch?.Stop();

                if(!string.IsNullOrEmpty(successMessage))
                {
                    var message = measureTime && stopwatch != null
                        ? $"{successMessage} ({stopwatch.Elapsed.TotalSeconds:F1}s)"
                        : successMessage;

                    ExecuteOnUIThread(() => ShowMessage(message, StatusMessageType.Success, successAutoHideSeconds));
                }
            } catch(Exception ex)
            {
                stopwatch?.Stop();

                var message = !string.IsNullOrEmpty(errorMessage) ? errorMessage : $"İşlem başarısız: {ex.Message}";

                ExecuteOnUIThread(() => ShowMessage(message, StatusMessageType.Error, autoHideSeconds: -1));
                throw;
            }
        }
        #endregion

        #region Private Methods
        private void ExecuteOnUIThread(Action action)
        {
            if(_dispatcherQueue != null)
                _dispatcherQueue.TryEnqueue(() => action());
            else
                action();
        }

        private void CancelAutoHide()
        {
            _autoHideCts?.Cancel();
            _autoHideCts?.Dispose();
            _autoHideCts = null;
        }

        private void SetupAutoHide(int seconds)
        {
            if(seconds > 0)
            {
                _autoHideCts = new CancellationTokenSource();
                _ = AutoHideAfterDelay(TimeSpan.FromSeconds(seconds), _autoHideCts.Token);
            }
        }

        private async Task AutoHideAfterDelay(TimeSpan delay, CancellationToken ct)
        {
            try
            {
                await Task.Delay(delay, ct);
                if(!ct.IsCancellationRequested)
                    Clear();
            } catch(TaskCanceledException)
            {
            } finally
            {
                _autoHideCts?.Dispose();
                _autoHideCts = null;
            }
        }

        private string SanitizeMessage(string message)
        {
            if(string.IsNullOrEmpty(message))
                return string.Empty;

            return message
                .Replace("\r\n", " ")
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();
        }

        private void NotifyPropertyChanged(string propertyName)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion
    }
}