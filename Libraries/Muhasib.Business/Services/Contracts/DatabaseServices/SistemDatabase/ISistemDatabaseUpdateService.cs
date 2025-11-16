namespace Muhasib.Business.Services.Contracts.DatabaseServices.SistemDatabase
{
    /// <summary>
    /// Sistem database güncelleme ve versiyon yönetimi servisi.
    /// Sistem database'leri için update, maintenance ve version synchronization işlemlerini koordine eder.
    /// </summary>
    public interface ISistemDatabaseUpdateService
    {
        /// <summary>
        /// Tüm sistem database'lerini initialize eder ve sağlık kontrolü yapar.
        /// </summary>
        Task<bool> InitializeAllDatabasesAsync();

        /// <summary>
        /// Tüm sistem database'lerinin versiyonlarını kontrol eder.
        /// </summary>
        Task<bool> CheckAllDatabaseVersionsAsync();

        /// <summary>
        /// Sistemin genel durum raporunu oluşturur.
        /// </summary>
        Task<string> GetOverallSystemStatusAsync();

        /// <summary>
        /// Versiyon kontrolü ile birlikte sistem database'ini initialize eder.
        /// </summary>
        Task<bool> InitializeWithVersionCheckAsync(string databaseName);

        /// <summary>
        /// Sistem versiyonlarını senkronize eder.
        /// </summary>
        Task<bool> SynchronizeSystemVersionsAsync(string databaseName);

        /// <summary>
        /// Tüm sistem database'lerini günceller.
        /// </summary>
        Task<bool> UpdateAllSystemDatabasesAsync();

        /// <summary>
        /// Güncelleme gerektiren database'leri listeler.
        /// </summary>
        Task<List<string>> GetDatabasesNeedingUpdateAsync();

        /// <summary>
        /// Sistem database bakım işlemlerini çalıştırır.
        /// </summary>
        Task<bool> RunSystemDatabaseMaintenanceAsync();
    }
}
