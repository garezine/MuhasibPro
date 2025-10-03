using Microsoft.UI.Dispatching;
using Muhasebe.Business.Common;
using Muhasebe.Business.Services.Abstracts.Common;
using Muhasebe.Domain.Enum;
using MuhasibPro.Contracts.CommonServices;
using MuhasibPro.Helpers;
using System.Diagnostics;

namespace MuhasibPro.Infrastructure.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        private Stopwatch _stopwatch = new();

        public ViewModelBase(ICommonServices commonServices)
        {
            StatusBarHelpers.Initialize(DispatcherQueue.GetForCurrentThread());
            ContextService = commonServices.ContextService;
            NavigationService = commonServices.NavigationService;
            MessageService = commonServices.MessageService;
            DialogService = commonServices.DialogService;
            LogService = commonServices.LogService;
            NotificationService = commonServices.NotificationService;

            // MessageService'e context'i kaydet (WinUI 3 için)
            if (MessageService is IMessageService msgService)
            {
                MessageService.RegisterContext(ContextService.ContextId, ContextService);
            }
            StatusBar = StatusBarHelpers.Instance;
        }

        public StatusBarHelpers StatusBar { get; private set; }

        public IContextService ContextService { get; }
        public INavigationService NavigationService { get; }
        public IMessageService MessageService { get; }
        public IDialogService DialogService { get; }
        public ILogService LogService { get; }
        public INotificationService NotificationService { get; set; }

        public bool IsMainWindow => ContextService.IsMainView;

        private bool _isBusy = false;
        public bool IsBusy { get => _isBusy; set => Set(ref _isBusy, value); }

        public virtual string Title => string.Empty;

        #region Log Metodları

        public void LogAppInformation(string source, string action, string message, string description)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await LogService.AppLogService.WriteAsync(LogType.Bilgi, source, action, message, description);
                }
                catch { /* Logging hatası sessizce ignore */ }
            });
        }

        public void LogSistemInformation(string source, string action, string message, string description)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await LogService.SistemLogService.WriteAsync(LogType.Bilgi, source, action, message, description);
                }
                catch { /* Logging hatası sessizce ignore */ }
            });
        }

        public void LogAppWarning(string source, string action, string message, string description)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await LogService.AppLogService.WriteAsync(LogType.Dikkat, source, action, message, description);
                }
                catch { /* Logging hatası sessizce ignore */ }
            });
        }

        public void LogSistemWarning(string source, string action, string message, string description)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await LogService.SistemLogService.WriteAsync(LogType.Dikkat, source, action, message, description);
                }
                catch { /* Logging hatası sessizce ignore */ }
            });
        }

        public void LogAppException(string source, string action, Exception exception)
        { LogAppError(source, action, exception.Message, exception.ToString()); }

        public void LogSistemException(string source, string action, Exception exception)
        { LogSistemError(source, action, exception.Message, exception.ToString()); }

        public void LogAppError(string source, string action, string message, string description)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await LogService.AppLogService.WriteAsync(LogType.Hata, source, action, message, description);
                }
                catch { /* Logging hatası sessizce ignore */ }
            });
        }

        public void LogSistemError(string source, string action, string message, string description)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await LogService.SistemLogService.WriteAsync(LogType.Hata, source, action, message, description);
                }
                catch { /* Logging hatası sessizce ignore */ }
            });
        }

        #endregion

        #region Status Metodları - Yeni Tip Destekli

        // ============================================
        // YENİ METODLAR - Tip Destekli
        // ============================================

        /// <summary>
        /// Başarı mesajı göster (yeşil, 3 saniye otomatik kapanır)
        /// </summary>
        public void StatusSuccess(string message)
        {
            MessageService.Send(this, "StatusSuccess", message);
        }

        /// <summary>
        /// Uyarı mesajı göster (turuncu, 4 saniye otomatik kapanır)
        /// </summary>
        public void StatusWarning(string message)
        {
            MessageService.Send(this, "StatusWarning", message);
        }

        /// <summary>
        /// Progress göster (belirsiz süre)
        /// </summary>
        public void StatusProgress(string message = "İşlem yapılıyor...")
        {
            StatusBar.ShowProgress(message);
        }

        /// <summary>
        /// Progress göster (yüzdelik)
        /// </summary>
        public void StatusProgress(string message, double percent)
        {
            StatusBar.ShowProgress(message, percent);
        }

        /// <summary>
        /// Progress güncelle
        /// </summary>
        public void UpdateProgress(double percent)
        {
            StatusBar.UpdateProgress(percent);
        }

        /// <summary>
        /// Progress'i gizle
        /// </summary>
        public void HideProgress()
        {
            StatusBar.HideProgress();
        }

        // ============================================
        // MEVCUT METODLAR - Geriye Uyumluluk
        // ============================================

        public void StatusReady()
        {
            MessageService.Send(this, "StatusMessage", "Hazır");
        }

        public void StatusMessage(string message)
        {
            MessageService.Send(this, "StatusMessage", message);
        }

        public void StatusError(string message)
        {
            MessageService.Send(this, "StatusError", message);
        }

        // ============================================
        // ASYNC VERSİYONLAR
        // ============================================

        public async Task StatusSuccessAsync(string message)
        {
            if (MessageService is IMessageService msgService)
                await msgService.SendAsync(this, "StatusSuccess", message);
            else
                MessageService.Send(this, "StatusSuccess", message);
        }

        public async Task StatusWarningAsync(string message)
        {
            if (MessageService is IMessageService msgService)
                await msgService.SendAsync(this, "StatusWarning", message);
            else
                MessageService.Send(this, "StatusWarning", message);
        }

        public async Task StatusReadyAsync()
        {
            if (MessageService is IMessageService msgService)
                await msgService.SendAsync(this, "StatusMessage", "Hazır");
            else
                MessageService.Send(this, "StatusMessage", "Hazır");
        }

        public async Task StatusMessageAsync(string message)
        {
            if (MessageService is IMessageService msgService)
                await msgService.SendAsync(this, "StatusMessage", message);
            else
                MessageService.Send(this, "StatusMessage", message);
        }

        public async Task StatusErrorAsync(string message)
        {
            if (MessageService is IMessageService msgService)
                await msgService.SendAsync(this, "StatusError", message);
            else
                MessageService.Send(this, "StatusError", message);
        }

        #endregion

        #region Stopwatch Metodları

        /// <summary>
        /// İşlem başlat ve mesaj göster
        /// </summary>
        public void StartStatusMessage(string message)
        {
            StatusMessage(message);
            _stopwatch.Reset();
            _stopwatch.Start();
        }

        /// <summary>
        /// İşlem bitir ve süreyi göster
        /// </summary>
        public void EndStatusMessage(string message)
        {
            _stopwatch.Stop();
            StatusMessage($"{message} ({_stopwatch.Elapsed.TotalSeconds:#0.000} saniye)");
        }

        /// <summary>
        /// İşlem başarıyla bitir ve süreyi göster
        /// </summary>
        public void EndStatusSuccess(string message)
        {
            _stopwatch.Stop();
            StatusSuccess($"{message} ({_stopwatch.Elapsed.TotalSeconds:#0.000} saniye)");
        }

        public async Task StartStatusMessageAsync(string message)
        {
            await StatusMessageAsync(message);
            _stopwatch.Reset();
            _stopwatch.Start();
        }

        public async Task EndStatusMessageAsync(string message)
        {
            _stopwatch.Stop();
            await StatusMessageAsync($"{message} ({_stopwatch.Elapsed.TotalSeconds:#0.000} saniye)");
        }

        public async Task EndStatusSuccessAsync(string message)
        {
            _stopwatch.Stop();
            await StatusSuccessAsync($"{message} ({_stopwatch.Elapsed.TotalSeconds:#0.000} saniye)");
        }

        #endregion

        #region View Enable/Disable Metodları

        public void EnableThisView(string message = null)
        {
            message = message ?? "Hazır";
            MessageService.Send(this, "EnableThisView", message);
        }

        public void DisableThisView(string message)
        {
            MessageService.Send(this, "DisableThisView", message);
        }

        public void EnableOtherViews(string message = null)
        {
            message = message ?? "Hazır";
            MessageService.Send(this, "EnableOtherViews", message);
        }

        public void DisableOtherViews(string message)
        {
            MessageService.Send(this, "DisableOtherViews", message);
        }

        public void EnableAllViews(string message = null)
        {
            message = message ?? "Hazır";
            MessageService.Send(this, "EnableAllViews", message);
        }

        public void DisableAllViews(string message)
        {
            MessageService.Send(this, "DisableAllViews", message);
        }

        public async Task EnableThisViewAsync(string message = null)
        {
            message = message ?? "Hazır";
            if (MessageService is IMessageService msgService)
                await msgService.SendAsync(this, "EnableThisView", message);
            else
                MessageService.Send(this, "EnableThisView", message);
        }

        public async Task DisableThisViewAsync(string message)
        {
            if (MessageService is IMessageService msgService)
                await msgService.SendAsync(this, "DisableThisView", message);
            else
                MessageService.Send(this, "DisableThisView", message);
        }

        public async Task EnableOtherViewsAsync(string message = null)
        {
            message = message ?? "Hazır";
            if (MessageService is IMessageService msgService)
                await msgService.SendAsync(this, "EnableOtherViews", message);
            else
                MessageService.Send(this, "EnableOtherViews", message);
        }

        public async Task DisableOtherViewsAsync(string message)
        {
            if (MessageService is IMessageService msgService)
                await msgService.SendAsync(this, "DisableOtherViews", message);
            else
                MessageService.Send(this, "DisableOtherViews", message);
        }

        public async Task EnableAllViewsAsync(string message = null)
        {
            message = message ?? "Hazır";
            if (MessageService is IMessageService msgService)
                await msgService.SendAsync(this, "EnableAllViews", message);
            else
                MessageService.Send(this, "EnableAllViews", message);
        }

        public async Task DisableAllViewsAsync(string message)
        {
            if (MessageService is IMessageService msgService)
                await msgService.SendAsync(this, "DisableAllViews", message);
            else
                MessageService.Send(this, "DisableAllViews", message);
        }

        #endregion
    }
}