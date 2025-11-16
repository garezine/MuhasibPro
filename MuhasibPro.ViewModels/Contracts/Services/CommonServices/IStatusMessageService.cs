using Muhasib.Domain.Enum;
using System.ComponentModel;

namespace MuhasibPro.ViewModels.Contracts.Services.CommonServices
{
    /// <summary>
    /// StatusBar mesaj yönetimi için servis
    /// </summary>
    public interface IStatusMessageService : INotifyPropertyChanged
    {
        #region Properties
        string StatusMessage { get; }
        StatusMessageType MessageType { get; }
        bool IsProgressVisible { get; }
        bool IsProgressIndeterminate { get; }
        double ProgressValue { get; }
        DateTime LastUpdateTime { get; }
        bool ShowProgressBar { get; }

        // Computed Properties
        string StatusIconGlyph { get; }
        string StatusColorHex { get; }
        string ProgressText { get; }
        string LastUpdateTimeText { get; }
        bool ShowStatusIcon { get; }
        #endregion

        #region Basic Methods
        /// <summary>
        /// Dispatcher'ı başlat (UI thread için)
        /// </summary>
        void Initialize(object dispatcher);

        /// <summary>
        /// Mesaj göster
        /// </summary>
        void ShowMessage(string message, StatusMessageType type, int autoHideSeconds);
         

        /// <summary>
        /// Mesajları temizle
        /// </summary>
        void Clear();
        #endregion

        #region Progress Methods
        /// <summary>
        /// Belirsiz progress göster (spinning)
        /// </summary>
        

        /// <summary>
        /// Yüzdelik progress göster
        /// </summary>
        void ShowProgress(string message, double progressPercent);

        /// <summary>
        /// Progress değerini güncelle
        /// </summary>
        void UpdateProgress(double progressPercent);

        /// <summary>
        /// Progress'i gizle
        /// </summary>
        void HideProgress();
        #endregion

        #region Advanced Async Methods
        /// <summary>
        /// Belirsiz progress ile async işlem yürüt
        /// </summary>
        Task ExecuteWithProgressAsync(
            Func<Task> action,
            string progressMessage,
            string successMessage = null,
            string errorMessage = null,
            bool measureTime = true,
            int successAutoHideSeconds = 3);

        /// <summary>
        /// Yüzdelik progress ile async işlem yürüt
        /// </summary>
        Task ExecuteWithProgressAsync(
            Func<IProgress<double>, Task> action,
            string progressMessage,
            string successMessage = null,
            string errorMessage = null,
            bool measureTime = true,
            int successAutoHideSeconds = 3);

        /// <summary>
        /// Async işlem yürüt (progress olmadan)
        /// </summary>
        Task ExecuteActionAsync(
            Func<Task> action,
            string startMessage = null,
            StatusMessageType startMessageType = StatusMessageType.Info,
            string successMessage = null,
            string errorMessage = null,
            bool measureTime = true,
            int successAutoHideSeconds = 3);
        #endregion
    }
}