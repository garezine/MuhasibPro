namespace Muhasib.Data.Managers.DatabaseManager.Models
{
    public class DatabaseHealthInfo
    {
        public bool CanConnect { get; set; }
        public int PendingMigrationsCount { get; set; }
        public int AppliedMigrationsCount { get; set; }
        public int BackupFilesCount { get; set; }
        public long DatabaseSize { get; set; }
        public DateTime? LastBackupDate { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }

        public string HealthStatus => HasError ? "❌ Hata" :
                                     !CanConnect ? "🔴 Bağlanamıyor" :
                                     PendingMigrationsCount > 0 ? "⚠️ Güncelleme Gerekli" :
                                     "✅ Sağlıklı";
    }
}