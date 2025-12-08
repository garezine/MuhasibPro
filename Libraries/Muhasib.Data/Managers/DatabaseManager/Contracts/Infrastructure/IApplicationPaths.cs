namespace Muhasib.Data.Managers.DatabaseManager.Contracts.Infrastructure
{
    public interface IApplicationPaths
    {
        // ============================================
        // BASE PATHS - ROOT Determination
        // ============================================
        string SanitizeDatabaseName(string databaseName);

        /// <summary>
        /// Kullanıcının AppData\Local\{appName} yolunu döndürür Production ortamında kullanılır
        /// </summary>
        string GetAppDataFolderPath();

        /// <summary>
        /// Geliştirme projesinin kök dizinini döndürür Development ortamında kullanılır
        /// </summary>
        bool SistemDatabaseFileExists();

        long GetSistemDatabaseSize();

        bool IsSistemDatabaseSizeValid();

        // ============================================
        // DATABASE STRUCTURE PATHS
        // [ROOT]/Databases/
        // ============================================

        /// <summary>
        /// [ROOT]/Databases/ klasör yolunu döndürür
        /// </summary>
        string GetDatabasesFolderPath();

        /// <summary>
        /// [ROOT]/Databases/Tenant/ klasör yolunu döndürür
        /// </summary>
        string GetTenantDatabasesFolderPath();

        /// <summary>
        /// [ROOT]/Databases/sistem.db dosya yolunu döndürür
        /// </summary>
        string GetSistemDatabaseFilePath();

        /// <summary>
        /// [ROOT]/Databases/Tenant/{databaseName}.db dosya yolunu döndürür Database adı güvenlik kontrollerinden geçer
        /// </summary>
        /// <exception cref="ArgumentException">Geçersiz database adı</exception>
        string GetTenantDatabaseFilePath(string databaseName);


        // ============================================
        // BACKUP STRUCTURE PATHS
        // [ROOT]/Backup/
        // ============================================

        /// <summary>
/// [ROOT]/Backup/ klasör yolunu döndürür
/// </summary>
        string GetBackupFolderPath();

        /// <summary>
        /// [ROOT]/Backup/Tenant/ klasör yolunu döndürür
        /// </summary>
        string GetTenantBackupFolderPath();


        // ============================================
        // TEMP STRUCTURE PATHS
        // [ROOT]/Temp/
        // ============================================

        /// <summary>
/// [ROOT]/Temp/ klasör yolunu döndürür
/// </summary>
        string GetTempFolderPath();


        // ============================================
        // HELPER METHODS
        // ============================================

        /// <summary>
/// Verilen database adının dosyasının var olup olmadığını kontrol eder
/// </summary>
        bool TenantDatabaseFileExists(string databaseName);

        bool IsTenantDatabaseSizeValid(string databaseName);

        long GetTenantDatabaseSize(string databaseName);

        /// <summary>
        /// Temp klasöründe benzersiz bir dosya yolu oluşturur
        /// </summary>
        /// <param name="extension">Dosya uzantısı (varsayılan: .tmp)</param>
        string GenerateUniqueTempFilePath(string extension = ".tmp");

        /// <summary>
        /// Belirtilen süreden eski temp dosyalarını temizler
        /// </summary>
        /// <param name="olderThan">Bu süreden eski dosyalar silinir</param>
        void CleanupTempFiles(TimeSpan olderThan);

        /// <summary>
        /// Database adını valide eder, geçersizse exception fırlatır
        /// </summary>
        /// <param name="databaseName">Valide edilecek database adı</param>
        /// <exception cref="ArgumentException">Geçersiz database adı</exception>
        void CleanupSqliteWalFiles(string databaseName);
    }
}