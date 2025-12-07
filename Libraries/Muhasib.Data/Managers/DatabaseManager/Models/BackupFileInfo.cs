namespace Muhasib.Data.Managers.DatabaseManager.Models
{
    public enum BackupType
    {
        Manual = 1,      // Kullanıcı manuel oluşturdu
        Automatic = 2,   // Schedule/otomatik backup
        Safety = 3,      // Restore öncesi güvenlik
        Migration = 4,   // Migration öncesi
        System = 5       // Sistem tarafından oluşturuldu
    }

    public class BackupFileInfo
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime CreatedDate { get; set; }
        public BackupType BackupType { get; set; } // ⭐ ENUM!
        public string DatabaseName { get; set; }

        public string FileSizeDisplay => FormatFileSize(FileSizeBytes);
        public string BackupTypeDisplay => GetBackupTypeDisplay(BackupType); // ⭐ Display property

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

        private string GetBackupTypeDisplay(BackupType type)
        {
            return type switch
            {
                BackupType.Manual => "Manuel",
                BackupType.Automatic => "Otomatik",
                BackupType.Safety => "Güvenlik",
                BackupType.Migration => "Migration",
                BackupType.System => "Sistem",
                _ => "Bilinmeyen"
            };
        }
    }
}