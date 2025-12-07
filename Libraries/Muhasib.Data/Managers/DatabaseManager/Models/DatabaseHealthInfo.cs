namespace Muhasib.Data.Managers.DatabaseManager.Models
{
    public class DatabaseHealthInfo
    {
        public string DatabaseName { get; set; }
        public DateTime CheckTime { get; set; } = DateTime.Now;

        // Core Health Metrics
        public bool CanConnect { get; set; }
        public bool DatabaseFileExists { get; set; }
        public int PendingMigrationsCount { get; set; }
        public int AppliedMigrationsCount { get; set; }

        // Additional Info
        public int BackupFilesCount { get; set; }
        public long DatabaseSize { get; set; }
        public DateTime? LastBackupDate { get; set; }

        // Error Handling
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }

        // Smart Status
        public string HealthStatus => HasError ? "❌ Hata" :
                                     !DatabaseFileExists ? "🔴 Dosya Yok" :
                                     !CanConnect ? "🔴 Bağlanamıyor" :
                                     PendingMigrationsCount > 0 ? "⚠️ Güncelleme Gerekli" :
                                     "✅ Sağlıklı";

        // Helper Properties
        public string DatabaseSizeDisplay => FormatFileSize(DatabaseSize);
        public bool NeedsAttention => HasError || !CanConnect || PendingMigrationsCount > 0;

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}