using System.ComponentModel;

namespace MuhasibPro.ViewModels.Contracts.Services.CommonServices
{
    /// <summary>
    /// StatusBar üst bilgileri yönetimi (User, Database, Save status)
    /// Mesaj işlemleri için IStatusMessageService kullanın
    /// </summary>
    public interface IStatusBarService : INotifyPropertyChanged
    {
        public IStatusMessageService StatusMessageService { get; set; }

        #region Properties
        /// <summary>
        /// Kullanıcı adı (StatusBar'da gösterilir)
        /// </summary>
        string UserName { get; set; }

        /// <summary>
        /// Veritabanı bağlantı mesajı
        /// </summary>
        string DatabaseConnectionMessage { get; set; }

        /// <summary>
        /// Veritabanı bağlantı durumu (true: bağlı, false: bağlı değil)
        /// </summary>
        bool IsDatabaseConnection { get; set; }

        
       
        #endregion

        #region Methods
        /// <summary>
        /// Dispatcher'ı başlat (UI thread için)
        /// </summary>
        void Initialize(object dispatcher);

        /// <summary>
        /// Kayıt durumunu ayarla
        /// </summary>
      

        /// <summary>
        /// Veritabanı durumunu ayarla
        /// </summary>
        void SetDatabaseStatus(bool isConnected, string message = null);
        #endregion
    }
}