namespace Muhasib.Data.Managers.DatabaseManager.Models
{
    public enum ConnectionTestResult
    {
        Success,
        DatabaseNotFound,
        ConnectionFailed,
        InvalidParameter,
        UnknownError,
        InvalidSchema,
        SqlServerUnavailable
    }
    public class TenantHealthInfo
    {
        public string DatabaseName { get; set; }
        public bool IsHealthy { get; set; }
        public string Message { get; set; }
        public DateTime CheckTime { get; set; }

        // Dosya bilgileri
        public bool FileExists { get; set; }
        public long FileSizeBytes { get; set; }
        public string FileSizeFormatted => FormatFileSize(FileSizeBytes);

        // Bağlantı bilgileri
        public bool CanConnect { get; set; }

        // Migration bilgileri
        public bool HasPendingMigrations { get; set; }
        public int PendingMigrationCount { get; set; }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int counter = 0;
            decimal number = bytes;

            while (Math.Round(number / 1024) >= 1 && counter < suffixes.Length - 1)
            {
                number /= 1024;
                counter++;
            }

            return $"{number:n2} {suffixes[counter]}";
        }
    }

}
